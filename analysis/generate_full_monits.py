import pandas as pd
from pathlib import Path

print("Starting synchronized log parsing from Windows PerfMon (General version)...\n")

# ================= CONFIGURATION =================
CORES_COUNT = 16  # IMPORTANT: Set your logical cores count for CPU normalization!
TARGET_FOLDER_NAME = 'Full'
PERFMON_FILENAME = 'perfmon_results.csv'
# ================================================

script_dir = Path(__file__).resolve().parent
repo_root = script_dir.parent

input_folder = repo_root / 'tests' / 'results' / TARGET_FOLDER_NAME
output_folder = script_dir / 'processed' / TARGET_FOLDER_NAME
perfmon_file = input_folder / 'usage' / PERFMON_FILENAME 

vusers_map = {'01': 1, '02': 20, '03': 100, '04': 500}

if not input_folder.exists():
    print(f"[ERROR] Input folder not found: {input_folder}")
    exit(1)

output_folder.mkdir(parents=True, exist_ok=True)

try:
    print(f"[INFO] Loading and cleaning PerfMon file from: {perfmon_file}...")
    df_perfmon = pd.read_csv(perfmon_file, quotechar='"', sep=',')
    
    # Rename the first column to 'Timestamp'
    time_col = df_perfmon.columns[0]
    df_perfmon.rename(columns={time_col: 'Timestamp'}, inplace=True)
    
    # Convert to datetime
    df_perfmon['Timestamp'] = pd.to_datetime(df_perfmon['Timestamp'], errors='coerce')
    
    # Clean whitespace and convert to numeric for all metric columns
    for col in df_perfmon.columns:
        if col != 'Timestamp':
            df_perfmon[col] = pd.to_numeric(df_perfmon[col].astype(str).str.strip(), errors='coerce').fillna(0)
            
except Exception as e:
    print(f"[FATAL] Critical error reading PerfMon file: {e}")
    exit(1)

results_data = []

for file_path in input_folder.glob('*.csv'):
    filename = file_path.name
    
    parts = filename.replace('.csv', '').split('_')
    if len(parts) < 4: continue
    
    profile_num = parts[-2]
    endpoint = parts[-3]
    
    # Build process key to find the correct column in PerfMon
    if parts[0] == 'Legacy':
        platform = 'IIS'
        process_key = 'w3wp'
    else:
        arch = 'Minimal' if parts[2] == 'MinimalApi' else 'Mvc'
        platform = f'Net{parts[1].replace("Net", "")}_{arch}'
        process_key = f'AdventureWorksReports.Modern.{parts[1]}.{arch}'

    vusers = vusers_map.get(profile_num)
    if not vusers: continue

    # Locate CPU and RAM columns for the specific process
    cpu_col = next((c for c in df_perfmon.columns if process_key in c and '% Processor Time' in c), None)
    ram_col = next((c for c in df_perfmon.columns if process_key in c and 'Working Set - Private' in c), None)

    if not cpu_col or not ram_col:
        print(f"    [WARNING] Skipped {filename}: Could not find columns for '{process_key}' in PerfMon.")
        continue

    try:
        df_jmeter = pd.read_csv(file_path, usecols=['timeStamp'])
        # Convert JMeter time (UTC ms) to local time (Warsaw) and strip timezone for PerfMon comparison
        local_times = pd.to_datetime(df_jmeter['timeStamp'], unit='ms', utc=True)\
                        .dt.tz_convert('Europe/Warsaw')\
                        .dt.tz_localize(None)
        
        # ADD TIME BUFFER: +/- 2 seconds
        dt_start = local_times.min() - pd.Timedelta(seconds=2)
        dt_end = local_times.max() + pd.Timedelta(seconds=2)

        # Filter PerfMon dataframe by the calculated time window
        time_mask = (df_perfmon['Timestamp'] >= dt_start) & (df_perfmon['Timestamp'] <= dt_end)
        phase_data = df_perfmon[time_mask]
        
        if not phase_data.empty:
            # RAM: Bytes to Megabytes
            max_ram_bytes = phase_data[ram_col].max()
            max_ram = round(max_ram_bytes / (1024 * 1024), 0)
            
            # CPU: Normalize by core count
            avg_cpu_raw = phase_data[cpu_col].mean()
            avg_cpu = round(avg_cpu_raw / CORES_COUNT, 1)
            
            results_data.append({
                'Endpoint': endpoint,
                'VUsers': vusers,
                'Platform': platform,
                'Max_RAM': max_ram,
                'Avg_CPU': avg_cpu
            })
            print(f"[{platform:15} | {endpoint:10} | {vusers:3} VU] RAM: {max_ram} MB | CPU: {avg_cpu}%")
        else:
            print(f"    [WARNING] No PerfMon data in the time window for: {filename}")

    except Exception as e:
        print(f"    [ERROR] Processing {filename}: {e}")

if results_data:
    df_results = pd.DataFrame(results_data)
    endpoints = df_results['Endpoint'].unique()
    
    for ep in endpoints:
        df_ep = df_results[df_results['Endpoint'] == ep]
        
        df_ep_low = df_ep[df_ep['VUsers'].isin([1, 20])]
        df_ep_high = df_ep[df_ep['VUsers'].isin([100, 500])]
        
        if not df_ep_low.empty:
            pivot_ram_low = df_ep_low.pivot(index='VUsers', columns='Platform', values='Max_RAM').fillna(0)
            pivot_cpu_low = df_ep_low.pivot(index='VUsers', columns='Platform', values='Avg_CPU').fillna(0)
            pivot_ram_low.to_csv(output_folder / f'latex_RAM_{ep}_1_20.csv')
            pivot_cpu_low.to_csv(output_folder / f'latex_CPU_{ep}_1_20.csv')
            
        if not df_ep_high.empty:
            pivot_ram_high = df_ep_high.pivot(index='VUsers', columns='Platform', values='Max_RAM').fillna(0)
            pivot_cpu_high = df_ep_high.pivot(index='VUsers', columns='Platform', values='Avg_CPU').fillna(0)
            pivot_ram_high.to_csv(output_folder / f'latex_RAM_{ep}_100_500.csv')
            pivot_cpu_high.to_csv(output_folder / f'latex_CPU_{ep}_100_500.csv')
            
    print("\n[SUCCESS] Saved synchronized LaTeX tables.")
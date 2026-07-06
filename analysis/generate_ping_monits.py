import pandas as pd
from pathlib import Path

print("Starting synchronized log parsing from Windows PerfMon (Ping version)...\n")

# ================= CONFIGURATION =================
CORES_COUNT = 16  
TARGET_FOLDER_NAME = 'Ping'
PERFMON_FILENAME = 'perfmon_results.csv'
# ================================================

series_map = {'0': '500_0', '1': '500_1', '2': '500_2'}

script_dir = Path(__file__).resolve().parent
repo_root = script_dir.parent

input_folder = repo_root / 'tests' / 'results' / TARGET_FOLDER_NAME
output_folder = script_dir / 'processed' / TARGET_FOLDER_NAME
perfmon_file = input_folder / 'usage' / PERFMON_FILENAME

if not input_folder.exists():
    print(f"[ERROR] Input folder not found: {input_folder}")
    exit(1)

output_folder.mkdir(parents=True, exist_ok=True)

try:
    print(f"[INFO] Loading and cleaning PerfMon file from: {perfmon_file}...")
    df_perfmon = pd.read_csv(perfmon_file, quotechar='"', sep=',')
    
    time_col = df_perfmon.columns[0]
    df_perfmon.rename(columns={time_col: 'Timestamp'}, inplace=True)
    df_perfmon['Timestamp'] = pd.to_datetime(df_perfmon['Timestamp'], errors='coerce')
    
    for col in df_perfmon.columns:
        if col != 'Timestamp':
            df_perfmon[col] = pd.to_numeric(df_perfmon[col].astype(str).str.strip(), errors='coerce').fillna(0)
            
except Exception as e:
    print(f"[FATAL] Critical error reading PerfMon file: {e}")
    exit(1)

results_data = []

for file_path in input_folder.glob('*.csv'):
    filename = file_path.name
    
    # E.g., Modern_Net8_MinimalApi_Ping_Max_Overhead_500_0.csv
    parts = filename.replace('.csv', '').split('_')
    if len(parts) < 6: 
        continue
    
    endpoint = parts[3]      # 'Ping'
    series_num = parts[-1]   # '0', '1' or '2'
    series_name = series_map.get(series_num, f"500_{series_num}")
    
    # Identify architecture and build process key
    arch = 'Minimal' if parts[2] == 'MinimalApi' else 'Mvc'
    platform = f'Net{parts[1].replace("Net", "")}_{arch}'
    process_key = f'AdventureWorksReports.Modern.{parts[1]}.{arch}'

    vusers = 500 

    cpu_col = next((c for c in df_perfmon.columns if process_key in c and '% Processor Time' in c), None)
    ram_col = next((c for c in df_perfmon.columns if process_key in c and 'Working Set - Private' in c), None)

    if not cpu_col or not ram_col:
        print(f"    [WARNING] Skipped {filename}: Could not find columns for '{process_key}' in PerfMon.")
        continue

    try:
        df_jmeter = pd.read_csv(file_path, usecols=['timeStamp'])
        local_times = pd.to_datetime(df_jmeter['timeStamp'], unit='ms', utc=True)\
                        .dt.tz_convert('Europe/Warsaw')\
                        .dt.tz_localize(None)
        
        # Time buffer +/- 2 seconds
        dt_start = local_times.min() - pd.Timedelta(seconds=2)
        dt_end = local_times.max() + pd.Timedelta(seconds=2)

        time_mask = (df_perfmon['Timestamp'] >= dt_start) & (df_perfmon['Timestamp'] <= dt_end)
        phase_data = df_perfmon[time_mask]
        
        if not phase_data.empty:
            max_ram_bytes = phase_data[ram_col].max()
            max_ram = round(max_ram_bytes / (1024 * 1024), 0)
            
            avg_cpu_raw = phase_data[cpu_col].mean()
            avg_cpu = round(avg_cpu_raw / CORES_COUNT, 1)
            
            results_data.append({
                'Series': series_name,
                'Platform': platform,
                'Max_RAM': int(max_ram),
                'Avg_CPU': avg_cpu
            })
            print(f"[{platform:15} | Series: {series_name:4}] RAM: {max_ram} MB | CPU: {avg_cpu}%")
        else:
            print(f"    [WARNING] No PerfMon data in the time window for: {filename}")

    except Exception as e:
        print(f"    [ERROR] Processing {filename}: {e}")

if results_data:
    df_results = pd.DataFrame(results_data)
    
    df_results.sort_values(by=['Series', 'Platform'], inplace=True)
    df_results.to_csv(output_folder / 'resource_usage_ping_summary.csv', index=False)
    
    pivot_ram = df_results.pivot(index='Series', columns='Platform', values='Max_RAM').fillna(0)
    pivot_cpu = df_results.pivot(index='Series', columns='Platform', values='Avg_CPU').fillna(0)
    
    pivot_ram.to_csv(output_folder / 'latex_ping_RAM_summary.csv')
    pivot_cpu.to_csv(output_folder / 'latex_ping_CPU_summary.csv')
            
    print(f"\n[SUCCESS] Synchronized resource usage results saved to: {output_folder.resolve()}")
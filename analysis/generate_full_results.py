import pandas as pd
from pathlib import Path

print("Starting data aggregation for LaTeX system...\n")

TARGET_FOLDER_NAME = 'Full'

script_dir = Path(__file__).resolve().parent
repo_root = script_dir.parent

input_folder = repo_root / 'tests' / 'results' / TARGET_FOLDER_NAME
output_folder = script_dir / 'processed' / TARGET_FOLDER_NAME

if not input_folder.exists():
    print(f"[ERROR] Input folder not found: {input_folder}")
    exit(1)

output_folder.mkdir(parents=True, exist_ok=True)

data_rows = []
vusers_map = {'01': 1, '02': 20, '03': 100, '04': 500}

for file_path in input_folder.glob('*.csv'):
    filename = file_path.name
    
    parts = filename.replace('.csv', '').split('_')
    profile_num = parts[-2]
    endpoint = parts[-3]
    
    if parts[0] == 'Legacy':
        platform = 'IIS'
    else:
        arch = 'Minimal' if parts[2] == 'MinimalApi' else 'MVC'
        platform = f'Net{parts[1].replace("Net", "")}_{arch}'

    vusers = vusers_map.get(profile_num, 0)
    
    try:
        df = pd.read_csv(file_path, usecols=['elapsed', 'timeStamp', 'success'])
        
        df_success = df[df['success'] == True]
        
        p95 = round(df_success['elapsed'].quantile(0.95), 2)
        p50 = round(df_success['elapsed'].quantile(0.50), 2)
        
        error_requests = len(df[df['success'] == False])
        error_rate = (error_requests / len(df)) * 100 if len(df) > 0 else 0
        
        total_requests = len(df)
        time_start = df['timeStamp'].min()
        time_end = df['timeStamp'].max()
        duration_seconds = (time_end - time_start) / 1000.0
        
        throughput = round(total_requests / duration_seconds, 2) if duration_seconds > 0 else 0
        good_requests = total_requests - error_requests
        goodput = round(good_requests / duration_seconds, 2) if duration_seconds > 0 else 0
        
        data_rows.append({
            'Endpoint': endpoint,
            'VUsers': vusers,
            'Platform': platform,
            'p95': p95,
            'p50': p50,
            'throughput': throughput,
            'goodput': goodput,
            'ErrorRate': error_rate
        })
    except Exception as e:
        print(f"    [WARNING] Skipped {filename}: {e}")

df_master = pd.DataFrame(data_rows)

if df_master.empty:
    print(f"[ERROR] No valid data found in {input_folder}. Exiting.")
    exit(1)

endpoints = df_master['Endpoint'].unique()

for ep in endpoints:
    df_ep = df_master[df_master['Endpoint'] == ep]
    
    df_ep_low = df_ep[df_ep['VUsers'].isin([1, 20])]
    df_ep_high = df_ep[df_ep['VUsers'].isin([100, 500])]
    
    if not df_ep_low.empty:
        pivot_p95_low = df_ep_low.pivot(index='VUsers', columns='Platform', values='p95').fillna(0)
        pivot_p95_low.to_csv(output_folder / f'latex_p95_{ep}_1_20.csv')
        
        pivot_throughput_low = df_ep_low.pivot(index='VUsers', columns='Platform', values='throughput').fillna(0)
        pivot_throughput_low.to_csv(output_folder / f'latex_throughput_{ep}_1_20.csv')
        
        pivot_goodput_low = df_ep_low.pivot(index='VUsers', columns='Platform', values='goodput').fillna(0)
        pivot_goodput_low.to_csv(output_folder / f'latex_goodput_{ep}_1_20.csv')
        
        pivot_error_rate_low = df_ep_low.pivot(index='VUsers', columns='Platform', values='ErrorRate').fillna(0)
        pivot_error_rate_low.to_csv(output_folder / f'latex_error_rate_{ep}_1_20.csv')
        
    if not df_ep_high.empty:
        pivot_p95_high = df_ep_high.pivot(index='VUsers', columns='Platform', values='p95').fillna(0)
        pivot_p95_high.to_csv(output_folder / f'latex_p95_{ep}_100_500.csv')
        
        pivot_throughput_high = df_ep_high.pivot(index='VUsers', columns='Platform', values='throughput').fillna(0)
        pivot_throughput_high.to_csv(output_folder / f'latex_throughput_{ep}_100_500.csv')
        
        pivot_goodput_high = df_ep_high.pivot(index='VUsers', columns='Platform', values='goodput').fillna(0)
        pivot_goodput_high.to_csv(output_folder / f'latex_goodput_{ep}_100_500.csv')
        
        pivot_error_rate_high = df_ep_high.pivot(index='VUsers', columns='Platform', values='ErrorRate').fillna(0)
        pivot_error_rate_high.to_csv(output_folder / f'latex_error_rate_{ep}_100_500.csv')
    
    print(f"    [INFO] Generated tabular data for: {ep}")

print(f"\nDone! Copy the generated 'latex_*.csv' files from '{output_folder.resolve()}' to your .tex folder.")
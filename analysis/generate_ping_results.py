import pandas as pd
from pathlib import Path

print("Starting JMeter results aggregation for summary statistics...\n")


TARGET_FOLDER_NAME = 'Ping'

script_dir = Path(__file__).resolve().parent
repo_root = script_dir.parent

input_folder = repo_root / 'tests' / 'results' / TARGET_FOLDER_NAME
output_folder = script_dir / 'processed' / TARGET_FOLDER_NAME
output_file = output_folder / 'statistics_summary.csv'

if not input_folder.exists():
    print(f"[ERROR] Input folder not found: {input_folder}")
    exit(1)

output_folder.mkdir(parents=True, exist_ok=True)

def analyze_jmeter_results():
    print(f"[INFO] Looking for CSV files in: {input_folder}...\n")
    csv_files = list(input_folder.glob("*.csv"))
    
    if not csv_files:
        print("[WARNING] No CSV files found for analysis!")
        return

    results_list = []

    for file_path in csv_files:
        filename = file_path.name
        
        try:
            df = pd.read_csv(file_path)
            
            if 'timeStamp' not in df.columns or 'elapsed' not in df.columns:
                print(f"    [WARNING] Skipped {filename} - missing required JMeter columns.")
                continue
                
            total_requests = len(df)
            if total_requests == 0:
                continue

            duration_s = (df['timeStamp'].max() - df['timeStamp'].min()) / 1000.0
            if duration_s == 0:
                duration_s = 1
                
            rps = total_requests / duration_s
            
            success_count = df['success'].astype(str).str.lower().eq('true').sum()
            error_rate = ((total_requests - success_count) / total_requests) * 100

            p50 = df['elapsed'].median()
            p95 = df['elapsed'].quantile(0.95)
            std_dev = df['elapsed'].std()
            
            results_list.append({
                'Application/Test': filename.replace('.csv', ''),
                'Throughput (RPS)': round(rps, 2),
                'p50 [ms]': round(p50, 2),
                'p95 [ms]': round(p95, 2),
                'StdDev [ms]': round(std_dev, 2),
                'Error Rate [%]': round(error_rate, 2),
                'Sample Count': total_requests
            })
            
        except Exception as e:
            print(f"    [ERROR] Failed to process {filename}: {e}")

    if not results_list:
        print("[WARNING] No valid data to summarize.")
        return

    # Create the final dataframe and display results
    df_results = pd.DataFrame(results_list)
    df_results.sort_values(by='Application/Test', inplace=True)
    
    print("--- STATISTICS SUMMARY READY ---")
    print(df_results.to_string(index=False))
    
    df_results.to_csv(output_file, index=False)
    print(f"\n[SUCCESS] Clean summary saved to: {output_file.resolve()}")

if __name__ == "__main__":
    analyze_jmeter_results()
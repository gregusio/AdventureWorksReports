# Performance Research: .NET Framework vs .NET 8 / 10

This repository contains the complete source code, test scripts (Apache JMeter), and analytical scripts (Python) 
used to evaluate throughput and resource utilization during the migration of applications from the 
.NET Framework (IIS) to the .NET 8/10 (Kestrel) platform.

## 🛠 Prerequisites

To run the tests successfully, the environment must meet the following requirements:
* **Operating System:** Windows 10/11 (required due to the IIS server and PerfMon mechanism).
* **Runtime Environment:** .NET 8.0 and .NET 10.0 SDKs, along with .NET Framework 4.8.
* **Database:** Docker Desktop (container named `adventureworks_db` with a SQL Server image).
* **Load Testing Tools:** Apache JMeter (version 5.6+).
* **Python 3.9+** with the `pandas` library installed (for result processing):
```bash
pip install pandas
```

---

## Step 1: Environment and Data Preparation

**1. Database:**
Ensure the Docker engine is running. Start the database environment using Docker Compose by running the following command in the directory containing your `docker-compose.yml` file:
```bash
docker compose up -d
```

**2. Applications (Build & Hosting):**
Before running any tests, ensure all projects are built in the **Release** configuration (e.g., using Visual Studio or the .NET CLI).
* The Legacy application (.NET Framework) must be compiled and hosted on a local IIS server under an application pool named `DefaultAppPool` (running on port 80).
* Modern applications (.NET 8/10) do not require external hosting, but they must be published first. The test script will run the compiled `.exe` files directly from their respective `bin/Release/netX.0/publish/` folders using the built-in Kestrel server.

**3. Generating Test Data (Payloads):**
Navigate to the `tests/payloads` folder and generate the JSON files used for POST endpoint testing (Bulk Reviews):

```bash
cd tests/payloads
python generate.py
```

---

## Step 2: Running Tests

The main orchestrator for the testing process is the PowerShell script `Run-Benchmarks.ps1`, located in the `tests` directory.
**Important:** The script must be run as an **Administrator** to properly recycle the IIS application pool.

### Available research scenarios (`-Scenario`):

### Available research scenarios (`-Scenario`):
* `Full` - Complete load testing phase (all endpoints, profiles up to 500 VUsers).
* `DatasOff` - Isolated tests with the "DATAS" mechanism disabled. **Important:** To execute this scenario, you must first switch your repository to the designated branch `datas-off`, then rebuild and republish the .NET 10 application before running the script.
* `Ping` - Lightweight test verifying the framework's baseline overhead.

### Execution Examples:

**Option A (Default):**
Runs the "Full" scenario, assuming JMeter is installed on the `C:\` drive:

```powershell
cd tests
.\Run-Benchmarks.ps1

```

**Option B (Custom path and selected scenario):**
If JMeter is installed elsewhere, pass the `-JMeterPath` parameter:

```powershell
.\Run-Benchmarks.ps1 -JMeterPath "D:\Tools\apache-jmeter\bin\jmeter.bat" -Scenario "DatasOff"

```

The raw `.csv` results will be automatically generated in the `tests/results/<ScenarioName>` folder.

---

## Step 3: Result Analysis

Analytical scripts are located in the `analysis` directory. 

**1. Generating throughput and latency tables (Full & DatasOff scenarios):**
To aggregate response times (p95) and throughput, open your terminal and run the script (ensure the `TARGET_FOLDER_NAME` variable in the code points to your desired scenario folder):
```bash
cd analysis
python generate_full_results.py

```

Output will be saved in the `analysis/processed/` directory.

> ** IMPORTANT: CPU Normalization**
> Before running either of the PerfMon synchronization scripts below, you **must** open them in a text editor (e.g., `generate_full_monits.py` and `generate_ping_monits.py`) and change the `CORES_COUNT` variable to match the number of logical processors on your specific test machine. This is critical for accurate CPU utilization calculations.

**2. Synchronization with hardware metrics (Full & DatasOff scenarios):**
If you collected data using Windows Performance Monitor during the tests (to analyze CPU and RAM usage), copy your PerfMon log file to `tests/results/<ScenarioName>/usage/perfmon_results.csv`, and then run:

```bash
python generate_full_monits.py

```

**3. Analyzing the Ping scenario:**
Due to the specific structure of the Ping tests (execution in grouped series instead of varying VUsers), use the dedicated scripts to process its data. Ensure the `TARGET_FOLDER_NAME` variable in both scripts is set to your Ping results folder, and run:

```bash
# To generate throughput, latency, and error rate summaries:
python generate_ping_results.py

# To synchronize and aggregate PerfMon hardware metrics (CPU/RAM):
python generate_ping_monits.py

```

All outputs for the Ping scenario will also be saved in the corresponding `analysis/processed/` directory.



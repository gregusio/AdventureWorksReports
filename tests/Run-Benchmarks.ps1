param (
    [string]$JMeterPath = "C:\apache-jmeter-5.6.3\bin\jmeter.bat",
    [string]$AppPoolName = "DefaultAppPool",
    [string]$Scenario = "Full" # All options: "Full", "DatasOff", "Ping"
)

$testsDir = $PSScriptRoot
$repoRootDir = (Get-Item $testsDir).Parent.FullName 

$testPlan = Join-Path $testsDir "test_plan.jmx"
$resultsDir = Join-Path $testsDir "results\$Scenario" 
$payloadsDir = Join-Path $testsDir "payloads"
$dbContainerName = "adventureworks_db" 

$dummyFile = Join-Path $payloadsDir "empty.txt"
$reviewsSmallFile = Join-Path $payloadsDir "reviews_bulk_small.json"
$reviewsLargeFile = Join-Path $payloadsDir "reviews_bulk_large.json"

if (-Not (Test-Path $JMeterPath)) {
    Write-Error "Could not find jmeter.bat at: $JMeterPath. Use the -JMeterPath parameter to provide the correct location."
    exit 1
}

if (-Not (Test-Path $payloadsDir)) { New-Item -ItemType Directory -Path $payloadsDir -Force | Out-Null }
if (-Not (Test-Path $dummyFile)) { New-Item -ItemType File -Path $dummyFile -Force | Out-Null }
if (-Not (Test-Path $resultsDir)) { New-Item -ItemType Directory -Path $resultsDir -Force | Out-Null }

$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:HEAP = "-Xms8g -Xmx8g"

$apps = @(
    @{ Name="Legacy_NetFramework"; Path="DefaultAppPool"; Port=80; IsIIS=$true },
    @{ 
        Name="Modern_Net8_Mvc"; 
        Path=(Join-Path $repoRootDir "src\AdventureWorksReports.Modern.Net8.Mvc\bin\Release\net8.0\publish\AdventureWorksReports.Modern.Net8.Mvc.exe"); 
        Port=5042; 
        IsIIS=$false 
    },
    @{ 
        Name="Modern_Net10_Mvc"; 
        Path=(Join-Path $repoRootDir "src\AdventureWorksReports.Modern.Net10.Mvc\bin\Release\net10.0\publish\AdventureWorksReports.Modern.Net10.Mvc.exe"); 
        Port=5043; 
        IsIIS=$false 
    },
    @{ 
        Name="Modern_Net8_MinimalApi"; 
        Path=(Join-Path $repoRootDir "src\AdventureWorksReports.Modern.Net8.Minimal\bin\Release\net8.0\publish\AdventureWorksReports.Modern.Net8.Minimal.exe"); 
        Port=5044; 
        IsIIS=$false 
    }
)

$endpoints = @(
    @{ Name="MonthlyProfit"; Path="/api/reports/monthly-profit"; Method="GET" },
    @{ Name="CustomerHistory"; Path="/api/reports/customer-history"; Method="GET" },
    @{ Name="InventoryCsv"; Path="/api/reports/inventory-csv"; Method="GET" },
    @{ Name="BulkReviews"; Path="/api/reviews/bulk"; Method="POST" }
)

$profiles = @(
    @{ Name="01_Base"; VUsers=1; RampUp=1; Loops=100; IsDuration=$false },
    @{ Name="02_LowTraffic"; VUsers=20; RampUp=5; Loops=200; IsDuration=$false },
    @{ Name="03_Load"; VUsers=100; RampUp=10; Duration=180; IsDuration=$true },
    @{ Name="04_Stressful"; VUsers=500; RampUp=50; Duration=180; IsDuration=$true }
)

if ($Scenario -eq "DatasOff") {
    Write-Host "    [INFO] Running a script: DATAS OFF..." -ForegroundColor Yellow
    
    $profiles = @(
        @{ Name="03_Load"; VUsers=100; RampUp=10; Duration=180; IsDuration=$true },
        @{ Name="04_Stressful"; VUsers=500; RampUp=50; Duration=180; IsDuration=$true }
    )
}
elseif ($Scenario -eq "Ping") {
    Write-Host "    [INFO] Running a script: PING..." -ForegroundColor Yellow
    
    $endpoints = @(
        @{ Name="Ping"; Path="/api/reports/ok"; Method="GET" } 
    )

    $profiles = @(
        @{ Name="Max_Overhead_500"; VUsers=500; RampUp=5; Duration=120; IsDuration=$true }
    )
}

if (-Not (Test-Path $resultsDir)) { 
    New-Item -ItemType Directory -Path $resultsDir -Force | Out-Null 
    Write-Host "    [INFO] A results directory has been created: $resultsDir" -ForegroundColor Green
}

function Clean-Environment {
    Write-Host "    [Cleanup] Killing lingering server processes (.exe)..." -ForegroundColor DarkYellow
    Stop-Process -Name "AdventureWorksReports.Modern*" -Force -ErrorAction SilentlyContinue
}

function Restart-Database {
    Write-Host "    [Database] Restarting Docker container ($dbContainerName)..." -ForegroundColor DarkYellow
    docker restart $dbContainerName | Out-Null
    Write-Host "    [Database] Waiting 12 seconds for SQL Server to start..." -ForegroundColor DarkGray
    Start-Sleep -Seconds 12 
}

function Wait-ForApplication {
    param (
        [int]$Port,
        [int]$TimeoutSeconds = 45
    )
    Write-Host "        [Warm-up] Listening on port $Port (Waiting for server readiness)..." -ForegroundColor DarkGray
    
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    while ($stopwatch.Elapsed.TotalSeconds -lt $TimeoutSeconds) {
        $connection = Test-NetConnection -ComputerName "localhost" -Port $Port -WarningAction SilentlyContinue
        
        if ($connection.TcpTestSucceeded) {
            $elapsed = [math]::Round($stopwatch.Elapsed.TotalSeconds, 2)
            Write-Host "        [Warm-up] Server reported readiness after $elapsed s!" -ForegroundColor Green
            return $true
        }
        Start-Sleep -Milliseconds 500
    }
    
    Write-Host "        [ERROR] Server did not start within $TimeoutSeconds seconds!" -ForegroundColor Red
    return $false
}

foreach ($profile in $profiles) {
    Write-Host "==================================================" -ForegroundColor Magenta
    Write-Host " STARTING LOAD PHASE: $($profile.Name)" -ForegroundColor Magenta
    Write-Host "==================================================" -ForegroundColor Magenta

    foreach ($endpoint in $endpoints) {
        Write-Host " ---> Testing endpoint: $($endpoint.Name) [$($endpoint.Method)]" -ForegroundColor Yellow

        foreach ($app in $apps) {
            Write-Host "      > Application: $($app.Name)" -ForegroundColor Cyan
            
            Clean-Environment
            Restart-Database

            if ($app.IsIIS) {
                Write-Host "        [IIS] Recycling application pool: $($app.Path)..." -ForegroundColor DarkGray
                $appcmdPath = "$env:windir\System32\inetsrv\appcmd.exe"
                if (Test-Path $appcmdPath) {
                    & $appcmdPath recycle apppool /apppool.name:$($app.Path) | Out-Null
                } else {
                    Write-Host "        [ERROR] appcmd.exe not found. Make sure you are running the script as Administrator!" -ForegroundColor Red
                }
            } else {
                Write-Host "        [Kestrel] Starting binary .exe file on port $($app.Port)..." -ForegroundColor DarkGray
                $startArgs = "--urls `"http://localhost:$($app.Port)`""
                $workingDir = Split-Path $app.Path
                Start-Process -FilePath $app.Path -ArgumentList $startArgs -WorkingDirectory $workingDir -WindowStyle Hidden
            }
            
            $isReady = Wait-ForApplication -Port $app.Port
            
            if (-Not $isReady) {
                Write-Host "        [WARNING] Skipping this test because the application did not respond!" -ForegroundColor Red
                continue 
            }
            
            Start-Sleep -Seconds 2

            $activePayload = $dummyFile 
            if ($endpoint.Name -eq "BulkReviews") {
                if ($profile.Name -eq "01_Bazowy") { # Poprawiłem warunek na "01_Bazowy" zgodnie z Twoją tablicą profili
                    $activePayload = $reviewsSmallFile
                    Write-Host "        [INFO] Using SMALL payload file." -ForegroundColor Blue
                } else {
                    $activePayload = $reviewsLargeFile
                    Write-Host "        [INFO] Using LARGE payload file." -ForegroundColor Blue
                }
            }

            $csvFile = "$resultsDir\$($app.Name)_$($endpoint.Name)_$($profile.Name).csv"
            Write-Host "        [JMeter] Generating traffic... Saving to: $csvFile" -ForegroundColor Green

            $jmeterArgs = @(
                "-n", 
                "-t", $testPlan,
                "-JappPort=$($app.Port)",
                "-JapiPath=$($endpoint.Path)",
                "-JhttpMethod=$($endpoint.Method)",
                "-JpayloadFile=$activePayload",
                "-Jusers=$($profile.VUsers)",
                "-Jrampup=$($profile.RampUp)",
                "-l", $csvFile
            ) 
            if ($profile.IsDuration) {
                $jmeterArgs += " -Jduration=$($profile.Duration) -Jloops=-1"
            } else {
                $jmeterArgs += " -Jloops=$($profile.Loops) -Jduration=99999"
            }

            $processArgs = $jmeterArgs.Split(' ')
            & $jmeterPath $processArgs
            
            Write-Host "        Completed test for $($app.Name)." -ForegroundColor DarkGreen
            Write-Host "--------------------------------------------------" -ForegroundColor DarkGray
        }
    }
}

Clean-Environment
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host " All research phases completed successfully!" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

RUN apt-get update && apt-get install -y curl && \
    mkdir -p /var/opt/mssql/backup && \
    curl -L -o /var/opt/mssql/backup/AdventureWorks.bak "https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks/AdventureWorks2022.bak"
    
USER mssql
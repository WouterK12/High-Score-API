@echo off
rem Usage: backup-database.bat [output-file]
setlocal
cd /d "%~dp0"

if "%MYSQL_DATABASE_NAME%"=="" (set "DATABASE_NAME=HighScoreAPIDev") else (set "DATABASE_NAME=%MYSQL_DATABASE_NAME%")
for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd_HHmmss"') do set "TIMESTAMP=%%i"

if not exist backups mkdir backups
set "OUTPUT_FILE=%~1"
if "%OUTPUT_FILE%"=="" set "OUTPUT_FILE=backups\%DATABASE_NAME%_%TIMESTAMP%.sql"

docker compose exec -T mysql sh -c "exec mysqldump -uroot -p\"$MYSQL_ROOT_PASSWORD\" --single-transaction --routines --triggers --databases %DATABASE_NAME%" > "%OUTPUT_FILE%.tmp"
if errorlevel 1 (
    del "%OUTPUT_FILE%.tmp"
    echo Backup failed.
    exit /b 1
)
move /y "%OUTPUT_FILE%.tmp" "%OUTPUT_FILE%" >nul

echo Backup written to %OUTPUT_FILE%

@echo off
rem Usage: restore-database.bat <backup-file>
setlocal
cd /d "%~dp0"

if "%~1"=="" goto usage
if not exist "%~1" goto usage

set /p "CONFIRM=This will overwrite the database with '%~1'. Continue? [y/N] "
if /i not "%CONFIRM%"=="y" (
    echo Aborted.
    exit /b 1
)

docker compose exec -T mysql sh -c "exec mysql -uroot -p\"$MYSQL_ROOT_PASSWORD\"" < "%~1"
if errorlevel 1 (
    echo Restore failed.
    exit /b 1
)

echo Database restored from %~1
exit /b 0

:usage
echo Usage: %~nx0 ^<backup-file^>
exit /b 1

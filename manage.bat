@echo off
setlocal

if "%1"=="up" (
    echo Starting the Crypto Exchange environment...
    docker-compose up -d
    goto end
) else if "%1"=="down" (
    echo Stopping and removing the Crypto Exchange environment...
    docker-compose down -v
    goto end
) else (
    echo Usage: %0 [up^|down]
)

:end
endlocal

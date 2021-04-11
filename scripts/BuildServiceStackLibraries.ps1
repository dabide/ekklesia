#!/usr/bin/env pwsh

Push-Location "$PSScriptRoot/../api"
docker build --target=ss-lib -o type=local,dest="$PSScriptRoot/../api" ./Ekklesia.Api
Pop-Location

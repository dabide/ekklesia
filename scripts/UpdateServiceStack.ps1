#!/usr/bin/env pwsh

$serviceStackVersion = git -c 'versionsort.suffix=-' ls-remote --exit-code --refs --sort='version:refname' --tags https://github.com/ServiceStack/ServiceStack '*.*.*' |
    Select-Object -Last 1 |
    ForEach-Object {
        $_.split("/")[2]
    }

if ([string]::IsNullOrEmpty($serviceStackVersion)) {
    throw
}

Push-Location "$PSScriptRoot/../api"

(Get-Content "./Ekklesia.Api/Dockerfile") `
    -replace '^ARG SS_VERSION=.*$', "ARG SS_VERSION=$serviceStackVersion" |
        Out-File "./Ekklesia.Api/Dockerfile"

Write-Host "The latest ServiceStack version is $serviceStackVersion"

docker build --target=ss-lib -o type=local,dest="$PSScriptRoot/../api" ./Ekklesia.Api
Pop-Location

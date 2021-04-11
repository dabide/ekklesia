#!/usr/bin/env pwsh

$serviceStackVersion = git -c 'versionsort.suffix=-' ls-remote --exit-code --refs --sort='version:refname' --tags https://github.com/ServiceStack/ServiceStack '*.*.*' |
    Select-Object -Last 1 |
    ForEach-Object {
        $_.split("/")[2]
    }

if ([string]::IsNullOrEmpty($serviceStackVersion)) {
    throw
}

(Get-Content "$PSScriptRoot/../api/Ekklesia.Api/Dockerfile") `
    -replace '^ARG SS_VERSION=.*$', "ARG SS_VERSION=$serviceStackVersion" |
        Out-File "$PSScriptRoot/../api/Ekklesia.Api/Dockerfile"

Write-Host "The latest ServiceStack version is $serviceStackVersion"
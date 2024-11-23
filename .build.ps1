<#
.DESCRIPTION
	TinyLogger build script, run using cmdlet Invoke-Build from module InvokeBuild
#>
param (
    [string]
    $Version
)

function GetTargetFrameworks([string] $projectPath) {
    $project = [xml](Get-Content $projectPath)
    $project.SelectNodes("//TargetFrameworks") | ForEach-Object {
        $_.InnerText -split ';'
    }
}

task AssertVersion {
    if (-not $Version) {
        throw "Specify version with -Version parameter"
    }
}

task DotnetToolRestore {
    exec {
        dotnet tool restore
    }
}

task DotnetRestore {
    exec {
        dotnet restore
    }
}

task DotnetFormat DotnetToolRestore, DotnetRestore, {
    exec {
        dotnet format --no-restore
    }
}

task DotnetFormatCheck DotnetToolRestore, DotnetRestore, {
    exec {
        dotnet format --no-restore --verify-no-changes
    }
}

task DotnetBuild DotnetRestore, {
    exec {
        dotnet build --no-restore
    }
}

task DotnetTest DotnetBuild, {
    Push-Location (Join-Path "test" "TinyLogger.Tests")

    GetTargetFrameworks "./TinyLogger.Tests.csproj" | ForEach-Object {
        exec {
            dotnet run --no-build --framework $_
        }
    }
}

task DotnetPack AssertVersion, {
    exec {
        dotnet pack .\src\TinyLogger\TinyLogger.csproj `
            --configuration Release `
            --output . `
            /p:ContinuousIntegrationBuild="true" `
            /p:EnableSourcelink="true" `
            /p:Version=$Version
    }
}

task Package DotnetPack

task . DotnetFormatCheck, DotnetBuild, DotnetTest

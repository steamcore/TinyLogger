<#
.DESCRIPTION
	TinyLogger build script, run using cmdlet Invoke-Build from module InvokeBuild
#>
param (
    [string]
    $Version
)

$artifactFolder = "artifacts"
$testResultsFolder = Join-Path $artifactFolder "testresults"

task AssertVersion {
    if (-not $Version) {
        throw "Specify version with -Version parameter"
    }
}

task DotnetRestore {
    exec {
        dotnet restore
    }
}

task DotnetFormat DotnetRestore, {
    exec {
        dotnet format --no-restore --exclude-diagnostics IDE0051 IDE0052
    }
}

task DotnetFormatCheck DotnetRestore, {
    exec {
        dotnet format --no-restore --verify-no-changes --exclude-diagnostics IDE0051 IDE0052
    }
}

task DotnetBuild DotnetRestore, {
    exec {
        dotnet build --no-restore
    }
}

task DotnetTest DotnetBuild, {
    exec {
        dotnet test `
            --disable-logo `
            --no-build `
            --results-directory $testResultsFolder
    }
}

task AotTest {
    $runtime = $IsWindows ? "win-x64" : "linux-x64"
    $binaryPath = Join-Path "artifacts" "publish" "TinyLogger.Tests" "release_$runtime"
    $binary = $IsWindows ? "$binaryPath/TinyLogger.Tests.exe" : "$binaryPath/TinyLogger.Tests"

    exec {
        dotnet publish (Join-Path "test" "TinyLogger.Tests") `
            --runtime $runtime `
            -p:"Aot=true"
    }

    exec {
        & $binary --disable-logo --results-directory $testResultsFolder
    }
}

task DotnetPack AssertVersion, {
    exec {
        dotnet pack (Join-Path "src" "TinyLogger") `
            --configuration Release `
            --output . `
            /p:ContinuousIntegrationBuild="true" `
            /p:EnableSourcelink="true" `
            /p:Version=$Version
    }
}

task Package DotnetPack

task . DotnetFormatCheck, DotnetBuild, DotnetTest

<#
.DESCRIPTION
	TinyLogger build script, run using cmdlet Invoke-Build from module InvokeBuild
#>
param (
    [string]
    $Version
)

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
        dotnet format --no-restore
    }
}

task DotnetFormatCheck DotnetRestore, {
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
    Push-Location "./test/TinyLogger.Tests"

    exec {
        dotnet test --no-build
    }
}

task AotTest {
    $binary = $IsWindows ? "./TinyLogger.Tests.exe" : "./TinyLogger.Tests"
    $runtime = $IsWindows ? "win-x64" : "linux-x64"

    Push-Location "./test/TinyLogger.Tests"
    exec {
        dotnet publish /p:"Aot=true" --runtime $runtime
    }
    Pop-Location

    Push-Location "./artifacts/publish/TinyLogger.Tests/release_net10.0_$runtime/"
    exec {
        & $binary --disable-logo
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

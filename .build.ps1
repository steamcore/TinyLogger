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
    Push-Location "./test/TinyLogger.Tests"

    $targetFrameworks = ([xml](Get-Content "./TinyLogger.Tests.csproj") | Select-Xml -XPath "//TargetFrameworks/text()").Node.Value -split ';'

    foreach ($framework in $targetFrameworks) {
        if ($framework -eq '$(TargetFrameworks)' -or ($framework -eq 'net481' -and -not $IsWindows)) {
            continue
        }
        exec {
            dotnet run --no-build --disable-logo --framework $framework
        }
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

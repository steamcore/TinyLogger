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
        dotnet format --fix-analyzers info --fix-style info --fix-whitespace
    }
}

task DotnetFormatCheck DotnetToolRestore, DotnetRestore, {
    exec {
        dotnet format --check --fix-analyzers info --fix-style info --fix-whitespace
    }
}

task DotnetBuild DotnetRestore, {
    exec {
        dotnet build --no-restore
    }
}

task DotnetTest DotnetBuild, {
    exec {
        dotnet test --no-build .\test\TinyLogger.Tests\TinyLogger.Tests.csproj
    }
}

task DotnetPack AssertVersion, {
    $outputPath = (Get-Item ".").FullName
    exec {
        dotnet pack .\src\TinyLogger\TinyLogger.csproj `
            --configuration Release `
            --output $outputPath `
            /p:ContinuousIntegrationBuild="true" `
            /p:EnableSourcelink="true" `
            /p:Version=$Version
    }
}

task Package DotnetPack

task . DotnetFormatCheck, DotnetBuild, DotnetTest

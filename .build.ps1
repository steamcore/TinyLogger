<#
.DESCRIPTION
	TinyLogger build script, run using cmdlet Invoke-Build from module InvokeBuild
#>
param (
    [string]
	$Version
)

task Test {
	exec { dotnet test .\test\TinyLogger.Tests\TinyLogger.Tests.csproj }
}

task AssertVersion {
	if (-not $Version) {
		throw "Specify version with -Version parameter"
	}
}

task Package {
	$outputPath = (Get-Item ".").FullName
	exec { dotnet pack .\src\TinyLogger\TinyLogger.csproj --configuration Release --output $outputPath /p:Version=$Version /p:EnableSourcelink="true" }
}

task . AssertVersion, Test, Package

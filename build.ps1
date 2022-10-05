param(${-no-msbuild})

$ErrorActionPreference = 'Stop'

$configuration = 'Release'

function BuildNETFramework {
	Write-Host 'Building .NET Framework x86 and x64 binaries'
	if (${-no-msbuild}) {
		dotnet build -v:m -c $configuration
		if ($LASTEXITCODE) { exit $LASTEXITCODE }
	}
	else {
		msbuild -v:m -m -restore -t:Build -p:Configuration=$configuration
		if ($LASTEXITCODE) { exit $LASTEXITCODE }
	}
}

function BuildNETCore {
	param([string]$architecture, [string]$targetframework)

	Write-Host "Building .NET $architecture binaries"

	$runtimeidentifier = "$architecture"

	if (${-no-msbuild}) {
		dotnet publish NETReactorSlayer.NETCore.Publish.slnf -v:m -c $configuration -f $targetframework -r $runtimeidentifier --self-contained -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=True -p:PublishSingleFile=true
		if ($LASTEXITCODE) { exit $LASTEXITCODE }
	}
	else {
		msbuild NETReactorSlayer.NETCore.Publish.slnf -v:m -m -restore -t:Publish -p:Configuration=$configuration -p:TargetFramework=$targetframework -p:RuntimeIdentifier=$runtimeidentifier -p:SelfContained=True -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=True -p:PublishSingleFile=true
		if ($LASTEXITCODE) { exit $LASTEXITCODE }
	}
}

BuildNETFramework
BuildNETCore win-x86 "net6.0"
BuildNETCore win-x86 "netcoreapp3.1"
BuildNETCore win-x64 "net6.0"
BuildNETCore linux-x64 "net6.0"
BuildNETCore osx-x64 "net6.0"
BuildNETCore win-x64 "netcoreapp3.1"
BuildNETCore linux-x64 "netcoreapp3.1"
BuildNETCore osx-x64 "netcoreapp3.1"
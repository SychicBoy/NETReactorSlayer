param(
	[ValidateSet("all", "netframework", "net6.0-win32", "net6.0-win64", "net6.0-win-arm", "net6.0-win-arm64", "net6.0-linux64", "net6.0-linux-arm", "net6.0-linux-arm64", "net6.0-osx64", "netcoreapp3.1-win32", "netcoreapp3.1-win64", "netcoreapp3.1-win-arm", "netcoreapp3.1-win-arm64", "netcoreapp3.1-linux64", "netcoreapp3.1-linux-arm", "netcoreapp3.1-linux-arm64", "netcoreapp3.1-osx64")]
	[string]$framework = 'all',
	${-no-msbuild}
)

$ErrorActionPreference = 'Stop'

$configuration = 'Release'

function BuildNETFramework {
	Write-Host 'Building .NET Framework x86 and x64 binaries'
	if (${-no-msbuild}) {
		dotnet build -v:m -c $configuration
		if ($LASTEXITCODE) { 
			Write-Host
			Write-Host ==========================
			Write-Host "THE BUILD OPERATION ENCOUNTERED AN ERROR. EXIT CODE: $LASTEXITCODE" -ForegroundColor Red
			exit $LASTEXITCODE 
		}
	}
	else {
		msbuild -v:m -m -restore -t:Build -p:Configuration=$configuration
		if ($LASTEXITCODE) { 
			Write-Host
			Write-Host ==========================
			Write-Host "THE BUILD OPERATION ENCOUNTERED AN ERROR. EXIT CODE: $LASTEXITCODE" -ForegroundColor Red
			exit $LASTEXITCODE 
		}
	}
}

function BuildNETCore {
	param([string]$architecture, [string]$tfm)

	Write-Host "Building .NET $architecture binaries"

	$runtimeidentifier = "$architecture"

	if (${-no-msbuild}) {
		dotnet publish NETReactorSlayer.NETCore.Publish.slnf -v:m -c $configuration -f $tfm -r $runtimeidentifier --self-contained -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=True -p:PublishSingleFile=true
		if ($LASTEXITCODE) { 
			Write-Host
			Write-Host ==========================
			Write-Host "THE BUILD OPERATION ENCOUNTERED AN ERROR. EXIT CODE: $LASTEXITCODE" -ForegroundColor Red
			exit $LASTEXITCODE 
		}
	}
	else {
		msbuild NETReactorSlayer.NETCore.Publish.slnf -v:m -m -restore -t:Publish -p:Configuration=$configuration -p:TargetFramework=$tfm -p:RuntimeIdentifier=$runtimeidentifier -p:SelfContained=True -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=True -p:PublishSingleFile=true
		if ($LASTEXITCODE) { 
			Write-Host
			Write-Host ==========================
			Write-Host "THE BUILD OPERATION ENCOUNTERED AN ERROR. EXIT CODE: $LASTEXITCODE" -ForegroundColor Red
			exit $LASTEXITCODE
		 }
	}
}

if($framework -eq 'all') {
	BuildNETFramework
	BuildNETCore win-x86 "net6.0"
	BuildNETCore win-x64 "net6.0"
	BuildNETCore win-arm "net6.0"
	BuildNETCore win-arm64 "net6.0"
	BuildNETCore linux-x64 "net6.0"
	BuildNETCore linux-arm "net6.0"
	BuildNETCore linux-arm64 "net6.0"
	BuildNETCore osx-x64 "net6.0"
	BuildNETCore win-x86 "netcoreapp3.1"
	BuildNETCore win-x64 "netcoreapp3.1"
	BuildNETCore win-arm "netcoreapp3.1"
	BuildNETCore win-arm64 "netcoreapp3.1"
	BuildNETCore linux-x64 "netcoreapp3.1"
	BuildNETCore linux-arm "netcoreapp3.1"
	BuildNETCore linux-arm64 "netcoreapp3.1"
	BuildNETCore osx-x64 "netcoreapp3.1"

	Write-Host
	Write-Host ==========================
	Write-Host "BUILD OPERATION COMPLETED SUCCESSFULLY" -ForegroundColor Green
}
else {
	if($framework -eq 'netframework'){
		BuildNETFramework
	}
	elseif($framework -eq 'net6.0-win32'){
		BuildNETCore win-x86 'net6.0'
	}
	elseif($framework -eq 'net6.0-win64'){
		BuildNETCore win-x64 'net6.0'
	}
	elseif($framework -eq 'net6.0-win-arm'){
		BuildNETCore win-arm 'net6.0'
	}
	elseif($framework -eq 'net6.0-win-arm64'){
		BuildNETCore win-arm64 'net6.0'
	}
	elseif($framework -eq 'net6.0-linux64'){
		BuildNETCore linux-x64 'net6.0'
	}
	elseif($framework -eq 'net6.0-linux-arm'){
		BuildNETCore linux-arm 'net6.0'
	}
	elseif($framework -eq 'net6.0-linux-arm64'){
		BuildNETCore linux-arm64 'net6.0'
	}
	elseif($framework -eq 'net6.0-osx64'){
		BuildNETCore osx-x64 'net6.0'
	}
	elseif($framework -eq 'netcoreapp3.1-win32'){
		BuildNETCore win-x86 'netcoreapp3.1'
	}
	elseif($framework -eq 'netcoreapp3.1-win64'){
		BuildNETCore win-x64 'netcoreapp3.1'
	}
	elseif($framework -eq 'netcoreapp3.1-win-arm'){
		BuildNETCore win-arm 'netcoreapp3.1'
	}
	elseif($framework -eq 'netcoreapp3.1-win-arm64'){
		BuildNETCore win-arm64 'netcoreapp3.1'
	}
	elseif($framework -eq 'netcoreapp3.1-linux64'){
		BuildNETCore linux-x64 'netcoreapp3.1'
	}
	elseif($framework -eq 'netcoreapp3.1-linux-arm'){
		BuildNETCore linux-arm 'netcoreapp3.1'
	}
	elseif($framework -eq 'netcoreapp3.1-linux-arm64'){
		BuildNETCore linux-arm64 'netcoreapp3.1'
	}
	elseif($framework -eq 'netcoreapp3.1-osx64'){
		BuildNETCore osx-x64 'netcoreapp3.1'
	}

	Write-Host
	Write-Host ==========================
	Write-Host "BUILD OPERATION COMPLETED SUCCESSFULLY" -ForegroundColor Green
}
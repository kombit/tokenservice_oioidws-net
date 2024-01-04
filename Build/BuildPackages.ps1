param(
    [string] [parameter(Mandatory = $true)] $version, 
    [string] [parameter(Mandatory = $true)] $assemblyVersion, 
    [switch] $pushPackages,
    [switch] $nugetApiKey,
    [string] [parameter(Mandatory = $false)] $apiKey
)

$ErrorActionPreference = "Stop"

if($pushPackages.IsPresent)
{
    write-host "pushing package Kombit.OioIdws.WscCore" -ForegroundColor Yellow
    dotnet nuget push $("Kombit.OioIdws.WscCore.$version.nupkg") -s https://api.nuget.org/v3/index.json -k $apiKey
}
else
{
    write-host "Restoring nuget packages" -ForegroundColor Yellow
    dotnet restore ..\Source\Digst.OioIdws.WscCore\Digst.OioIdws.WscCore.csproj

    write-host "Build Digst.OioIdws.WscCore project" -ForegroundColor Yellow
    dotnet build ..\Source\Digst.OioIdws.WscCore\Digst.OioIdws.WscCore.csproj --force --configuration Release -p:AssemblyVersion=$assemblyVersion -p:FileVersion=$version

    write-host "Building nuget package Kombit.OioIdws.WscCore" -ForegroundColor Yellow
    dotnet pack ..\Source\Digst.OioIdws.WscCore\Digst.OioIdws.WscCore.csproj -p:AssemblyInformationalVersion=$assemblyVersion -p:AssemblyVersion=$assemblyVersion -p:FileVersion=$version -p:PackageVersion=$version --output .\ --include-symbols --include-source --configuration Release
}
#
#    Script to bump the version in the project file metadata, then
#    build the package and publish it to github.
#
#	NB Assumes a Nuget.Config in or above the current path, with the 
#		required github auth token defined
#
# 	Example:
#		.\put2git2.ps1 -bumpVersion false
#
param (
    $infile = (Get-ChildItem . -Filter *.csproj |Select -First 1).Name,
	#"D:\projects\public\CommonClassesLib\CommonClassesLib5.nuspec",
    #$pubdir = "D:\_publish",
	$bumpVersion = $true,
	$pubConfig = "Release"
)

$pubdir = "bin\"+$pubConfig
    
#
# Determine the package version from csproj file
# Increment it and update the csproj file if $bumpVersion
#

$infileName = [System.IO.Path]::GetFileName($infile)
$text = Get-Content $infile

$version = "1.0.0.0"
$versionSufix = ""

$patt = "<Version>([0-9.]+)</Version>"
if (![regex]::IsMatch($text, $patt)) 
{
    $patt = "<VersionPrefix>([0-9.]+)</VersionPrefix>"
    $pattSuffix = "<VersionSuffix>(.*?)</VersionSuffix>"
    if ([regex]::IsMatch($text, $pattSuffix)) 
    {
        $versionSufix = "-" + [regex]::Match($text, $pattSuffix).Groups[1].Value
    }
}
$value = [regex]::Match($text, $patt).Groups[1].Value
$version = $value

if ($bumpVersion) 
{
	#$value
	$parts = $value -split '\.'
	#$parts
	$parts[-1] = [convert]::ToInt32($parts[-1], 10) + 1
	#$parts
	$newvalue = ""
	$sep = ""
	foreach ($p in $parts) 
	{
		$newvalue = $newvalue + $sep + $p
		$sep = "."
	}
	#$newvalue
	$text = $text.Replace( $value, $newvalue )
	#$text
	Set-Content -Path $infile -Value $text
    $version = $newvalue
}
$version = $version + $versionSufix

#
# Pack the package letting dotnet use the project file settings
#

$packed = & dotnet pack -o $pubdir --no-build

#
# puch the new package to the github repo
# requires the repository to be correctly set in the project file
# requires the auth key to be set in some nuget.config file  
#

#$packageFileName = $pubdir + '\' + $infileName.Replace(".nuspec", "." + $newvalue + ".nupkg") 
$packResultPattern = "Successfully created package '(.*?)'"
if (![regex]::IsMatch($packed, $packResultPattern)) 
{
    echo "pack seems to have failed!"
    echo $packed
    exit
}
$packageFileName = [regex]::Match($packed, $packResultPattern).Groups[1].Value

dotnet nuget push $packageFileName --source "github"

#nuget pack $infile -outputdirectory $pubdir
#$packageFileName = $pubdir + '\' + $infileName.Replace(".nuspec", "." + $newvalue + ".nupkg")
#nuget add $packageFileName -Source D:\_publish\packages -expand
pause
#
#    Script to bump the version in the nuspec metadata, then
#    build the package and publish it to github.
#
#	NB Assumes a Nuget.Config in or above the current path, with the 
#		required github token defined
#
# 	Example:
#		.\put2git.ps1 -bumpVersion false
#
param (
    $infile = (Get-ChildItem . -Filter *.nuspec |Select -First 1).Name,
	#"D:\projects\public\CommonClassesLib\CommonClassesLib5.nuspec",
    $pubdir = "D:\_publish",
	$bumpVersion = $true,
	$pubConfig = "Release"
)
#
#$packDir = $pubdir + '\packages'
$infileName = [System.IO.Path]::GetFileName($infile)
$text = Get-Content $infile

if ($bumpVersion) 
{
	$patt = "<version>([0-9.]+)</version>"
	$value = [regex]::Match($text, $patt).Groups[1].Value
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
}

dotnet pack -c $pubConfig -o $pubdir --no-build
$packageFileName = $pubdir + '\' + $infileName.Replace(".nuspec", "." + $newvalue + ".nupkg")
dotnet nuget push $packageFileName --source "github"

#nuget pack $infile -outputdirectory $pubdir
#$packageFileName = $pubdir + '\' + $infileName.Replace(".nuspec", "." + $newvalue + ".nupkg")
#nuget add $packageFileName -Source D:\_publish\packages -expand
pause
Param(
  [parameter(Mandatory=$false)][string]$repo="",
  [parameter(Mandatory=$false)][bool]$push=$false,
  [parameter(Mandatory=$false)][string]$apikey,
  [parameter(Mandatory=$false)][bool]$build=$true
)

# Paths
$packFolder = (Get-Item -Path "./" -Verbose).FullName
$rootFolder = Join-Path $packFolder ".."
$srcPath = Join-Path $packFolder "../framework"



$projects = (Get-Content "./Components")

function Pack($projectFolder,$projectName) {  
  Set-Location $projectFolder
  $releaseProjectFolder = (Join-Path $projectFolder "bin/Release")
  if (Test-Path $releaseProjectFolder)
  {
     Remove-Item -Force -Recurse $releaseProjectFolder
  }
  
   & dotnet restore
   & dotnet pack -c Release
   if ($projectName) {
    $projectPackPath = Join-Path $projectFolder ("/bin/Release/" + $projectName + ".*.nupkg")
   }else {
    $projectPackPath = Join-Path $projectFolder ("/bin/Release/" + $project + ".*.nupkg")
   }
   Move-Item -Force $projectPackPath $packFolder 
}

if ($build) {
  foreach($project in $projects) {
    if (-not $project.StartsWith("#")){
      $projectName = ($project -Split "/" )[-1]
      $projectFolder = Join-Path $srcPath $project
      Pack -projectFolder $projectFolder -projectName $projectName 
    }    
  }
  Set-Location $packFolder
}

if($push) {
    if ([string]::IsNullOrEmpty($apikey)){
        Write-Warning -Message "未设置nuget仓库的APIKEY"
		exit 1
	}
	[xml]$propsXml = Get-Content (Join-Path $rootFolder "common.props")
    $version = $propsXml.Project.PropertyGroup.Version
	foreach($project in $projects) {
      $projectName = ($project -Split "/" )[-1]
      & dotnet nuget push ($projectName + "." + $version + ".nupkg") -s $repo -k $apikey --skip-duplicate
    }
}
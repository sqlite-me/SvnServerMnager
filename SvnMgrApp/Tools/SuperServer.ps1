$inputed = Read-Host "If you want install ""SvnMgrservice"", input ""I"""
$inputed = $inputed.ToUpper()

if("I" -ne $inputed){
	"you have cancel install!"
	return
}

$location = Get-Location
$localPath = $location.Path
$configFile = "$localPath\SvnMgrApp.exe.config"

[Xml]$XmlAppConfig = Get-Content $configFile

$SvnMgrServer = $XmlAppConfig.configuration.superSocket.servers.server | Where-Object { $_.name -eq "SvnMgrServer" }
$outStr = "server port : " + $SvnMgrServer.port
$outStr

$newPort = Read-Host "Keep this port, press ""Enter"" KEY; Need new port, Input Int32 number"
$newPort = $newPort.Trim()

if($newPort.Length -gt 0){
	$iPort = 0
	if(-not ([Int32]::TryParse($newPort,[ref] $iPort))){
		$outStr = "the port need Int32, but """ + $newPort +""" is not Int32"
		Write-Error $outStr
		return;
	}
	$SvnMgrServer.port = $iPort.ToString()
	$XmlAppConfig.Save($configFile)
}

$configFile = "$localPath\SvnMgrservice.xml"

[Xml]$XmlContent = Get-Content $configFile
<#foreach($one in $XmlContent.service.env){
	if("SVNMGRAPP_HOME".Equals($one.name)){
		$one.value = "$localPath\"
		break
	}
}#>
$evn = $XmlContent.service.env | Where-Object { $_.name -eq "SVNMGRAPP_HOME" }
$evn.value = "$localPath\"
$XmlContent.Save($configFile)
#$XmlContent.Dispose()
Start-Sleep ¨Cs 1

.\SvnMgrservice.exe install
Start-Sleep ¨Cs 1
.\SvnMgrservice.exe start
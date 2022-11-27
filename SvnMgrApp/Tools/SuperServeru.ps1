$inputed = Read-Host "If you want uninstall ""SvnMgrservice"", input ""U"""
$inputed = $inputed.ToUpper()

if("U" -ne $inputed){
	"you have cancel uninstall!"
	return
}

.\SvnMgrservice.exe stop
Start-Sleep ¨Cs 3
.\SvnMgrservice.exe uninstall
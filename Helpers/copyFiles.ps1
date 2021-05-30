$IPAddress=$args[0]
$UserName=$args[1]
$Password=$args[2]
$SourceFolder=$args[3]
$DestFolder=$args[4]

Write-Host "IPAddress: $IPAddress - User: $UserName - SourceFolder: $SourceFolder - DestFolder: $DestFolder" 
try {
	$Password = ConvertTo-SecureString $Password -AsPlainText -Force
	$mycreds = New-Object System.Management.Automation.PSCredential($UserName, $Password)

	New-PSDrive -Name L -PSProvider FileSystem -Root \\$IPAddress\c$\$DestFolder -Credential $mycreds -Persist -ErrorAction Stop
	Copy-Item -Path $SourceFolder\* -Destination "L:\" -Recurse -PassThru -ErrorAction Stop
	Remove-PSDrive -Name L -ErrorAction Stop
}
catch {
	Write-Host "An error occurred:"
	Write-Host $_
	exit 1
}



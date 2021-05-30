$IPAddress=$args[0]
$UserName=$args[1]
$Password=$args[2]
$SettingFolder=$args[3]
$NewValue=$args[4]

Write-Host "IPAddress: $IPAddress - User: $UserName - SettingFolder: $SettingFolder - NewValue: $NewValue" 
try {
	$Password = ConvertTo-SecureString $Password -AsPlainText -Force
	$mycreds = New-Object System.Management.Automation.PSCredential($UserName, $Password)
    $newValue = "Name=`"RegressionName`" Value=`"$NewValue`"></Setting>"
	New-PSDrive -Name H -PSProvider FileSystem -Root \\$IPAddress\c$\$SettingFolder -Credential $mycreds -Persist -ErrorAction Stop
	((Get-Content -Path "H:\settings.xml" -Raw) -replace 'Name="RegressionName" Value=".*"></Setting>',$newValue) | Set-Content -Path "H:\settings.xml" -ErrorAction Stop
	Remove-PSDrive -Name H -ErrorAction Stop
}
catch {
	Write-Host "An error occurred:"
	Write-Host $_
	exit 1
}
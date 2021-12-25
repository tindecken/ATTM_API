$IPAddress=$args[0]
$UserName=$args[1]
$Password=$args[2]
$SourceFolder=$args[3]
$DestFolder=$args[4]

Write-Host "IPAddress: $IPAddress - User: $UserName - SourceFolder: $SourceFolder - DestFolder: $DestFolder" 
try {
	$Password = ConvertTo-SecureString $Password -AsPlainText -Force
	$mycreds = New-Object System.Management.Automation.PSCredential($UserName, $Password)

	$session = New-PSSession -ComputerName $IPAddress -Credential $mycreds

	Copy-Item -Path $SourceFolder\* -Destination $DestFolder -ToSession $session -Recurse -PassThru -Force -ErrorAction Stop
}
catch {
	Write-Host "An error occurred while copying:"
	Write-Host $_
	exit 1
}
finally {
	
}
$IPAddress=$args[0]
$UserName=$args[1]
$Password=$args[2]
$SourceFolder=$args[3]
$DestFolder=$args[4]

write-host "IPAddress: $IPAddress - User: $UserName - Password: $Password - SourceFolder: $SourceFolder - DestFolder: $DestFolder" 

$Password = ConvertTo-SecureString $Password -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential($UserName, $Password)

New-PSDrive -Name L -PSProvider FileSystem -Root \\$IPAddress\c$\$DestFolder -Credential $mycreds -Persist
Copy-Item -Path $SourceFolder\* -Destination "L:\" -Recurse -PassThru
Remove-PSDrive -Name L
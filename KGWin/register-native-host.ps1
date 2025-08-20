# PowerShell script to register the native messaging host
# Run this script as Administrator

$manifestPath = Join-Path $PSScriptRoot "KGWin\com.kgconnect.native.json"
$exePath = Join-Path $PSScriptRoot "KGWin\bin\Debug\net8.0-windows10.0.19041.0\KGWin.exe"

# Update the manifest with the correct path
$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
$manifest.path = $exePath
$manifest | ConvertTo-Json -Depth 10 | Set-Content $manifestPath

Write-Host "Registering native messaging host..."
Write-Host "Manifest path: $manifestPath"
Write-Host "Executable path: $exePath"

# Register for Chrome
$chromeRegistryPath = "HKLM:\SOFTWARE\Google\Chrome\NativeMessagingHosts\com.kgconnect.native"
Write-Host "Registering for Chrome..."
New-Item -Path $chromeRegistryPath -Force | Out-Null
Set-ItemProperty -Path $chromeRegistryPath -Name "(Default)" -Value $manifestPath
Write-Host "Chrome registry key: $chromeRegistryPath"

# Register for Edge
$edgeRegistryPath = "HKLM:\SOFTWARE\Microsoft\Edge\NativeMessagingHosts\com.kgconnect.native"
Write-Host "Registering for Edge..."
New-Item -Path $edgeRegistryPath -Force | Out-Null
Set-ItemProperty -Path $edgeRegistryPath -Name "(Default)" -Value $manifestPath
Write-Host "Edge registry key: $edgeRegistryPath"

Write-Host "Native messaging host registered successfully for both Chrome and Edge!"
Write-Host "Registry value: $manifestPath"

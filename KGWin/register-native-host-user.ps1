# PowerShell script to register the native messaging host (User Level - No Admin Required)
# This registers in HKCU (Current User) instead of HKLM (Local Machine)

$manifestPath = Join-Path $PSScriptRoot "com.kgconnect.native.json"
$exePath = Join-Path $PSScriptRoot "bin\Debug\net8.0-windows10.0.19041.0\KGWin.exe"

# Update the manifest with the correct path
$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
$manifest.path = $exePath
$manifest | ConvertTo-Json -Depth 10 | Set-Content $manifestPath

Write-Host "Registering native messaging host (User Level)..."
Write-Host "Manifest path: $manifestPath"
Write-Host "Executable path: $exePath"

# Register for Chrome (User Level)
$chromeRegistryPath = "HKCU:\SOFTWARE\Google\Chrome\NativeMessagingHosts\com.kgconnect.native"
Write-Host "Registering for Chrome (User Level)..."
New-Item -Path $chromeRegistryPath -Force | Out-Null
Set-ItemProperty -Path $chromeRegistryPath -Name "(Default)" -Value $manifestPath
Write-Host "Chrome registry key: $chromeRegistryPath"

# Register for Edge (User Level)
$edgeRegistryPath = "HKCU:\SOFTWARE\Microsoft\Edge\NativeMessagingHosts\com.kgconnect.native"
Write-Host "Registering for Edge (User Level)..."
New-Item -Path $edgeRegistryPath -Force | Out-Null
Set-ItemProperty -Path $edgeRegistryPath -Name "(Default)" -Value $manifestPath
Write-Host "Edge registry key: $edgeRegistryPath"

Write-Host "Native messaging host registered successfully for both Chrome and Edge (User Level)!"
Write-Host "Registry value: $manifestPath"
Write-Host "Note: This registration is for the current user only and doesn't require administrator privileges."

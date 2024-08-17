$CURRENT_USER = $env:USERNAME
$HOME_PATH = $env:USERPROFILE

# Kill the ProcRespawn process if it's running
Get-Process -Name "ProcRespawn" | Stop-Process -Force

# Remove the installation directory
Remove-Item -Path "$HOME_PATH\.popemkt\proc-respawn" -Recurse -Force

# Remove the startup shortcut
Remove-Item -Path "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\ProcRespawn.lnk" -Force -ErrorAction SilentlyContinue

Write-Host "ProcRespawn has been uninstalled."
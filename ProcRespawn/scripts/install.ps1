$CURRENT_USER = $env:USERNAME
$HOME_PATH = $env:USERPROFILE

# Installation folder
New-Item -Path "$HOME_PATH\.popemkt\proc-respawn" -ItemType Directory -Force

# Publish the .NET application
# WinExe is to work around the issue of console window showing up when running the application
dotnet publish --runtime win-x64 -p:PublishSingleFile=true -p:OutputType=WinExe -c Release -o "$HOME_PATH\.popemkt\proc-respawn" --self-contained ..

# Note: The following lines are commented out as they are Linux-specific and don't have direct PowerShell equivalents
# #sudo mv "$HOME/.popemkt/proc-respawn/ProcRespawn" /usr/local/bin/ProcRespawn
# #sudo restorecon -Rv /usr/local/bin in SELinux, you need this to restore the permissions context

# Note: The systemd service part is removed as it's not applicable to Windows

# Create and modify the shortcut file (equivalent to .desktop file in Windows)
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\ProcRespawn.lnk")
$Shortcut.TargetPath = "$HOME_PATH\.popemkt\proc-respawn\ProcRespawn.exe"
$Shortcut.Save()

# Replace {USER} in the shortcut (if needed)
#(Get-Content "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\ProcRespawn.lnk") -replace '{USER}', $CURRENT_USER | Set-Content "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\ProcRespawn.lnk"
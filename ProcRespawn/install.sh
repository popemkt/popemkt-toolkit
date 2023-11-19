#!/bin/sh

CURRENT_USER=$(whoami)
HOME_PATH=$(echo $HOME)
#Installation folder
mkdir -p "$HOME_PATH/.popemkt-toolkit/proc-respawn"
dotnet publish --runtime linux-x64 -p:PublishSingleFile=true -c Release -o "$HOME/.popemkt-toolkit/proc-respawn" --self-contained 
#sudo mv "$HOME/.popemkt-toolkit/proc-respawn/ProcRespawn" /usr/local/bin/ProcRespawn
#sudo restorecon -Rv /usr/local/bin in SELinux, you need this to restore the permissions context

#This if for if you use this as a systemd service, which doesn't seem to work very well
#Fill service template
#sed -i 's/{USER}/'"$CURRENT_USER"'/g' proc-respawn.service
#sudo cp proc-respawn.service "/etc/systemd/system/"

#Create desktop file
sed -i 's/{USER}/'"$CURRENT_USER"'/g' proc-respawn.desktop
cp proc-respawn.desktop "$HOME_PATH/.config/autostart/"

#Reload and start
#sudo systemctl daemon-reload
#sudo systemctl enable proc-respawn.service
#sudo systemctl start proc-respawn.service
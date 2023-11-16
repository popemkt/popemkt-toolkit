#!/bin/sh

CURRENT_USER=$(whoami)
HOME_PATH=$(echo $HOME)
#Installation folder
mkdir -p "$HOME_PATH/.hoang-toolkit/proc-respawn"
dotnet publish --runtime linux-x64 -p:PublishSingleFile=true -c Release -o "$HOME/.hoang-toolkit/proc-respawn" --self-contained 
sudo mv "$HOME/.hoang-toolkit/proc-respawn/ProcRespawn" /usr/local/bin/ProcRespawn
sudo restorecon -Rv /usr/local/bin

#Fill service template
sed -i 's/{USER}/'"$CURRENT_USER"'/g' proc-respawn.service
sudo cp proc-respawn.service "/etc/systemd/system/"

#Reload and start
sudo systemctl daemon-reload
sudo systemctl enable proc-respawn.service
sudo systemctl start proc-respawn.service
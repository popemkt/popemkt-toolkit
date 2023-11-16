#!/bin/sh

CURRENT_USER=$(whoami)
HOME_PATH=$(echo $HOME)
#Installation folder
mkdir -p "$HOME_PATH/.hoang-toolkit/proc-respawn"
dotnet publish -c Release -o "$HOME_PATH/.hoang-toolkit/proc-respawn"
sudo ln -sf "$HOME_PATH/.hoang-toolkit/proc-respawn/ProcRespawn" /usr/local/bin/ProcRespawn
sudo chown -h "$CURRENT_USER":"$CURRENT_USER" /usr/local/bin/ProcRespawn

#Fill service template
sed -i 's/{USER}/'"$CURRENT_USER"'/g' proc-respawn.service
sudo cp proc-respawn.service "/etc/systemd/system/"

#Reload and start
sudo systemctl daemon-reload
sudo systemctl enable proc-respawn.service
sudo systemctl start proc-respawn.service
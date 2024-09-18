#!/bin/sh

CURRENT_USER=$(whoami)
HOME_PATH=$(echo $HOME)
#Installation folder
mkdir -p "$HOME_PATH/.popemkt/proc-respawn"
dotnet publish --runtime linux-x64 -p:PublishSingleFile=true -c Release -o "$HOME/.popemkt/proc-respawn" --self-contained ..
# Replace {USER} in the appsettings.{any env}.json files
for file in "$HOME_PATH/.popemkt/proc-respawn"/appsettings.*.json; do
    sed -i 's/{USER}/'"$CURRENT_USER"'/g' "$file"
done
#sudo mv "$HOME/.popemkt/proc-respawn/ProcRespawn" /usr/local/bin/ProcRespawn
#sudo restorecon -Rv /usr/local/bin in SELinux, you need this to restore the permissions context

#This if for if you use this as a systemd service, which doesn't seem to work very well
#Fill service template
#sed -i 's/{USER}/'"$CURRENT_USER"'/g' proc-respawn.service
#sudo cp proc-respawn.service "/etc/systemd/system/"

#Create desktop file
cp ../proc-respawn.desktop "$HOME_PATH/.config/autostart/"
sed -i 's/{USER}/'"$CURRENT_USER"'/g' "$HOME_PATH/.config/autostart/proc-respawn.desktop"

#Reload and start
#sudo systemctl daemon-reload
#sudo systemctl enable proc-respawn.service
#sudo systemctl start proc-respawn.service
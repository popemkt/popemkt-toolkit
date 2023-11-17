#!/bin/sh

CURRENT_USER=$(whoami)
HOME_PATH=$(echo $HOME)
pkill ProcRespawn
rm -rf "$HOME_PATH/.hoang-toolkit/proc-respawn"
rm "$HOME_PATH/.config/autostart/proc-respawn.desktop"
#systemctl disable proc-respawn.service
#systemctl stop proc-respawn.service
#rm "/etc/systemd/system/proc-respawn.service"
#systemctl reset-failed
#systemctl daemon-reload



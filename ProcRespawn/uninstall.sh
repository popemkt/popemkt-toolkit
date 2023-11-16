#!/bin/sh

rm -rf "$HOME/.hoang-toolkit/proc-respawn"
rm /usr/local/bin/ProcRespawn

systemctl disable proc-respawn.service
systemctl stop proc-respawn.service
rm "/etc/systemd/system/proc-respawn.service"
systemctl daemon-reload



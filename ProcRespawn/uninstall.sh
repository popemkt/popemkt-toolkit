#!/bin/sh

rm -rf "/home/popemkt/.hoang-toolkit/proc-respawn"
rm /usr/local/bin/ProcRespawn

systemctl disable proc-respawn.service
systemctl stop proc-respawn.service
rm "/etc/systemd/system/proc-respawn.service"
systemctl reset-failed
systemctl daemon-reload



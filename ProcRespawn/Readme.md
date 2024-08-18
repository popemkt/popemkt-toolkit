## Description

Proc-respawn is a simple tool to always respawn a process when it's stopped.

It works by keeping a list of to-respawn processes (and their binary/desktop locations) as configuration in `appsettings.json`

It serves people who doesn't want to accidentally close their always-on apps, like me.

## Installation

Clone this repo and run `./scripts/install.sh`

For uninstall just run `./scripts/uninstall.sh`

## Technical details
## Limitations

It's preferrable to work as a systemd unit, but such service is not suitable for launching GUI apps.

Right now it's being installed as an app with autostart desktop file: `proc-respawn.desktop`

## Past efforts 

This works well if not started from systemd init

It looks like GUI apps are not really supported from systemd init https://stackoverflow.com/questions/41459360/cannot-launch-gui-programs-from-systemd-service

Other options are `upstart`, autostart or maybe more indepth configuration of unit files

## TODO
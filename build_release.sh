#!/bin/bash

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
cd $SCRIPT_DIR

docker build . --tag kerbalmod_ci:latest
docker run -v $SCRIPT_DIR:/mnt/kerbalmod kerbalmod_ci:latest make release.tar.gz
docker run -v $SCRIPT_DIR:/mnt/kerbalmod kerbalmod_ci:latest make release.zip
chown -R $SUDO_USER:$SUDO_USER *

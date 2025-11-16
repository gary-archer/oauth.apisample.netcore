#!/bin/bash

#######################
# Build the API locally
#######################

cd "$(dirname "${BASH_SOURCE[0]}")"

#
# Build the app
#
dotnet build
if [ $? -ne 0 ]; then
  echo 'Problem encountered building the API'
  exit 1
fi

#
# Ensure that the logs folder exists
#
if [ ! -d './logs' ]; then
  mkdir './logs'
fi

#
# On Linux ensure that the API has permissions to listen on a port below 1024
#
if [ "$(uname -s)" == 'Linux' ]; then
  sudo setcap 'cap_net_bind_service=+ep' ./bin/Debug/net10.0/finalapi
fi

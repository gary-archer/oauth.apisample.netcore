#!/bin/bash

##################################################################################
# Build wiremock and grant it access to listen on a low privilege port if required
##################################################################################

cd "$(dirname "${BASH_SOURCE[0]}")"
cd wiremock

#
# Build the standalone Wiremock
#
dotnet build
if [ $? -ne 0 ]; then
  echo 'Problem encountered building Wiremock'
  read -n 1
  exit 1
fi

#
# On Linux ensure that Wiremock has permissions to listen on a port below 1024
#
if [ "$(uname -s)" == 'Linux' ]; then
  sudo setcap 'cap_net_bind_service=+ep' ./bin/Debug/net8.0/wiremock
fi

#!/bin/bash

##############################################
# A script to run Wiremock in a child terminal
##############################################

cd "$(dirname "${BASH_SOURCE[0]}")"
cd wiremock

#
# Build the standalone Wiremock if it has not been built yet
#
if [ ! -f ./bin/Debug/net8.0/wiremock ]; then
  dotnet build
  if [ $? -ne 0 ]; then
    echo 'Problem encountered building Wiremock'
    read -n 1
    exit
  fi
fi

#
# On Linux ensure that you have first granted Wiremock permissions to listen on a port below 1024
#
if [ "$(uname -s)" == 'Linux' ]; then
  sudo setcap 'cap_net_bind_service=+ep' ./bin/Debug/net8.0/wiremock
fi

#
# Run Wiremock over HTTPS in this terminal
#
dotnet run
if [ $? -ne 0 ]; then
  echo 'Problem encountered running Wiremock'
  read -n 1
  exit
fi

#
# Prevent automatic terminal closure
#
read -n 1
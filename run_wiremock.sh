#!/bin/bash

##############################################
# A script to run Wiremock in a child terminal
##############################################

cd "$(dirname "${BASH_SOURCE[0]}")"
cd wiremock

#
# Build the standalone Wiremock if it has not been built yet
#
if [ ! -f ./bin/Debug/net7.0/wiremock ]; then
  dotnet build
  if [ $? -ne 0 ]; then
    echo 'Problem encountered building Wiremock'
    read -n 1
    exit
  fi
fi

#
# Run Wiremock over HTTPS in this terminal
# On Linux ensure that you have first granted wiremock permissions to listen on port 3447:
# - sudo setcap 'cap_net_bind_service=+ep' ./wiremock/bin/Debug/net7.0/wiremock
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
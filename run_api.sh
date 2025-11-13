#!/bin/bash

##################################################
# A script to run the API in a particular terminal
##################################################

cd "$(dirname "${BASH_SOURCE[0]}")"

#
# Delete unwanted files that Visual Studio Code generates
#
rm oauth.apisample.netcore.sln 2>/dev/null

#
# Build the app
#
dotnet build
if [ $? -ne 0 ]; then
  echo 'Problem encountered building the API'
  read -n 1
  exit 1
fi

#
# On Linux ensure that the API has permissions to listen on a port below 1024
#
if [ "$(uname -s)" == 'Linux' ]; then
  sudo setcap 'cap_net_bind_service=+ep' ./bin/Debug/net10.0/finalapi
fi

#
# Ensure that the logs folder exists
#
if [ ! -d './logs' ]; then
  mkdir './logs'
fi

#
# Run the previously built API
#
dotnet run --no-build
if [ $? -ne 0 ]; then
  echo 'Problem encountered running the API'
  read -n 1
  exit 1
fi

#
# Prevent automatic terminal closure
#
read -n 1

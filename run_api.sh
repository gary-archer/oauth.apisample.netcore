#!/bin/bash

#############################################
# A script to run the API in a child terminal
#############################################

cd "$(dirname "${BASH_SOURCE[0]}")"

#
# Build the API's code
#
dotnet build
if [ $? -ne 0 ]; then
  echo 'Problem encountered building the API'
  read -n 1
  exit
fi

#
# Ensure that log folders exist
#
if [ ! -d '../oauth.logs' ]; then
  mkdir '../oauth.logs'
fi
if [ ! -d '../oauth.logs/api' ]; then
  mkdir '../oauth.logs/api'
fi

#
# On Linux ensure that you have first granted the API permissions to listen on a port below 1024
#
if [ "$(uname -s)" == 'Linux' ]; then
  sudo setcap 'cap_net_bind_service=+ep' ./bin/Debug/net8.0/sampleapi
fi

#
# Run the API in this terminal
#
dotnet run
if [ $? -ne 0 ]; then
  echo 'Problem encountered running the API'
  read -n 1
  exit
fi

#
# Prevent automatic terminal closure
#
read -n 1

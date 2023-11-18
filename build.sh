#!/bin/bash

#######################
# Build the API locally
#######################

cd "$(dirname "${BASH_SOURCE[0]}")"

rm -rf bin

#
# Build the app
#
dotnet build
if [ $? -ne 0 ]; then
  echo 'Problem encountered building the API'
  exit 1
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
# On Linux ensure that the API has permissions to listen on a port below 1024
#
if [ "$(uname -s)" == 'Linux' ]; then
  sudo setcap 'cap_net_bind_service=+ep' ./bin/Debug/net8.0/sampleapi
fi

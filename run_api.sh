#!/bin/bash

##################################################
# A script to run the API in a particular terminal
##################################################

cd "$(dirname "${BASH_SOURCE[0]}")"

#
# Download development SSL certificates if required
# Then configure the operating system to trust the root CA at certs/authsamples-dev.ssl.p12
#
./downloadcerts.sh
if [ $? -ne 0 ]; then
  exit
fi

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
  sudo setcap 'cap_net_bind_service=+ep' ./bin/Debug/net8.0/sampleapi
fi

#
# Ensure that folders for log files exist
#
if [ ! -d '../oauth.logs' ]; then
  mkdir '../oauth.logs'
fi
if [ ! -d '../oauth.logs/api' ]; then
  mkdir '../oauth.logs/api'
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

#!/bin/bash

###########################################################################
# A script to download SSL certificates, then build and run the API locally
###########################################################################

cd "$(dirname "${BASH_SOURCE[0]}")"

#
# Download development SSL certificates if required
# You need to ensure that the operating system trusts the file downloaded to ./certs/authsamples-dev.ca.pem
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
  exit
fi

#
# Ensure that log folders exist
#
if [ ! -d '../oauth.logs' ]; then
  mkdir '../oauth.logs'
fi
if [ -d '../oauth.logs/api' ]; then
  rm -rf '../oauth.logs/api'
fi
mkdir '../oauth.logs/api'

#
# Then start listening
# On Linux ensure that you have first granted Node.js permissions to listen on port 446:
# - sudo setcap 'cap_net_bind_service=+ep' ./bin/Debug/netcoreapp6/sampleapi
#
dotnet run
if [ $? -ne 0 ]; then
  echo 'Problem encountered running the API'
  exit
fi
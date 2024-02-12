#!/bin/bash

###########################################################################
# A script to download SSL certificates, then build and run the API locally
###########################################################################

cd "$(dirname "${BASH_SOURCE[0]}")"

#
# Download development SSL certificates if required
# You must then configure the host operating system to trust the file at ./certs/authsamples-dev.ca.pem
#
./downloadcerts.sh
if [ $? -ne 0 ]; then
  exit 1
fi

#
# Ensure that the development configuration is used
#
cp deployment/environments/dev/api.config.json ./api.config.json

#
# Build the API code
#
./build.sh
if [ $? -ne 0 ]; then
  exit 1
fi

#
# Then run the API
#
./run_api.sh
if [ $? -ne 0 ]; then
  exit 1
fi

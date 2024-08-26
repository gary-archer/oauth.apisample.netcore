#!/bin/bash

###########################################################################
# A script to download SSL certificates, then build and run the API locally
###########################################################################

cd "$(dirname "${BASH_SOURCE[0]}")"

#
# Ensure that the development configuration is used
#
cp deployment/environments/dev/api.config.json ./api.config.json

#
# Download development SSL certificates if required
# Then configure the operating system to trust the root CA at certs/authsamples-dev.ssl.p12
#
./downloadcerts.sh
if [ $? -ne 0 ]; then
  read -n 1
  exit 1
fi

#
# Run the previously built API
#
./run_api.sh

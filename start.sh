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
# Create development SSL certificates if required
#
./certs/create.sh
if [ $? -ne 0 ]; then
  exit 1
fi

#
# Run the previously built API
#
./run_api.sh

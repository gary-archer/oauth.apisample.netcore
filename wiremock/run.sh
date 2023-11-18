#!/bin/bash

##############################################
# A script to run Wiremock in a child terminal
##############################################

cd "$(dirname "${BASH_SOURCE[0]}")"
cd wiremock

#
# Run Wiremock over HTTPS in this terminal
#
dotnet run
if [ $? -ne 0 ]; then
  echo 'Problem encountered running Wiremock'
  read -n 1
  exit 1
fi

#
# Prevent automatic terminal closure
#
read -n 1
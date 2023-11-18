#!/bin/bash

##################################################
# A script to run the API in a particular terminal
##################################################

cd "$(dirname "${BASH_SOURCE[0]}")"

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

#!/bin/bash

##############################################################
# A script to test Docker deployment on a development computer
##############################################################

cd "$(dirname "${BASH_SOURCE[0]}")"
cd ../..

#
# Create development SSL certificates if required
#
./certs/create.sh
if [ $? -ne 0 ]; then
  exit 1
fi

#
# Build the code
#
dotnet publish finalapi.csproj -c Release -r linux-x64 --no-self-contained
if [ $? -ne 0 ]; then
  echo '*** .NET API build problem encountered'
  exit 1
fi

#
# Build the docker image
#
docker build -t finalnetcoreapi:latest .
if [ $? -ne 0 ]; then
  echo 'Problem encountered building the API docker image'
  exit
fi

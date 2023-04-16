#!/bin/bash

##############################################################
# A script to test Docker deployment on a development computer
##############################################################

cd "$(dirname "${BASH_SOURCE[0]}")"
cd ../..

#
# Download certificates if required
#
./downloadcerts.sh
if [ $? -ne 0 ]; then
  exit
fi

#
# Build the code
#
dotnet publish sampleapi.csproj -c Release -r linux-x64 --no-self-contained
if [ $? -ne 0 ]; then
  echo '*** .NET API build problem encountered'
  exit 1
fi

#
# Build the docker image
#
docker build -f deployment/docker-local/Dockerfile -t finalnetcoreapi:v1 .
if [ $? -ne 0 ]; then
  echo 'Problem encountered building the API docker image'
  exit
fi

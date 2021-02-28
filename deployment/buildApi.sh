#!/bin/bash

#
# A script to build our API into a docker image
#

#
# Point to the minikube api profile
#
minikube profile api
eval $(minikube docker-env --profile api)

#
# Build to DLLs and Linux libraries
#
echo "Building C# Code ..."
dotnet clean ../sampleapi.csproj
dotnet publish ../sampleapi.csproj -c Release -r linux-x64
if [ $? -ne 0 ]
then
  echo "*** C# build error ***"
  exit 1
fi

#
# Build the docker image, with libraries plus other resources
#
echo "Building .Net Core Docker Image ..."
cd ..
docker build --no-cache -f deployment/Dockerfile -t netcoreapi:v1 .
if [ $? -ne 0 ]
then
  echo "*** Docker build error ***"
  exit 1
fi

#
# Indicate success
#
cd deployment
echo "Build completed successfully"

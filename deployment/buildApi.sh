#!/bin/bash

#
# A script to build our API into a docker image
#

#
# Use the Minikube Docker Daemon rather than that of Docker Desktop for Mac
#
eval $(minikube docker-env)

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
docker build -f deployment/Dockerfile -t netcoreapi:v1 .
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

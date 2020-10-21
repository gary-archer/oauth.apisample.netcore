#!/bin/bash

#
# A MacOS script to deploy 2 instances of the C# API to a local PC Minikube Kubernetes cluster
#
echo "Building C# Code ..."
dotnet clean ../sampleapi.csproj
dotnet publish ../sampleapi.csproj -c Release -r linux-musl-x64
if [ $? -ne 0 ]
then
  echo "*** C# build error ***"
  exit 1
fi

#
# Clean up any resources for the previously deployed version of the API
#
kubectl delete deploy/netcoreapi   2>/dev/null
kubectl delete svc/netcoreapi-svc  2>/dev/null
docker image rm -f netcoreapi      2>/dev/null

#
# Build the docker image, with libraries plus other resources
#
echo "Building .Net Core Docker Image ..."
cd ..
docker build -f deployment/Dockerfile -t netcoreapi .
if [ $? -ne 0 ]
then
  echo "*** Docker build error ***"
  exit 1
fi

#
# Deploy 2 instances of the local docker image to 2 Kubernetes pods
#
echo "Deploying Docker Image to Kubernetes ..."
cd deployment
kubectl create -f Kubernetes.yaml
if [ $? -ne 0 ]
then
  echo "*** Kubernetes deployment error ***"
  exit 1
fi

#
# Expose the API to clients outside Kubernetes on port 443 with a custom host name
# We can then access the API at https://netcoreapi.mycompany.com/api/companies
#
kubectl apply -f ingress.yaml
echo "Deployment completed successfully"

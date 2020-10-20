#!/bin/bash

#
# A MacOS script to build the C# API and deploy it to a local PC minikube Kubernetes cluster
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
# Use the minikube docker daemon rather than that of Docker Desktop for Mac
#
echo "Preparing Kubernetes ..."
eval $(minikube docker-env)

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
# Deploy the local docker image to multiple Kubernetes pods
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
# Output POD details then the external address
#
echo "Deployment completed successfully"
kubectl get pod -l app=netcoreapi
API_URL=$(minikube service --url netcoreapi-svc)/api/companies
echo $API_URL

#
# Troubleshooting commands from outside Kubernetes
#
#curl $API_URL
#kubectl describe service netcoreapi-svc
#kubectl logs --tail=100 pod/netcoreapi-74f57df659-2tjz5

#
# Troubleshooting commands from inside the POD
#
#kubectl exec --stdin --tty pod/netcoreapi-74f57df659-2tjz5 -- /bin/sh
#ls -lr /usr/sampleapi
#apk add curl
#curl http://localhost/api/companies

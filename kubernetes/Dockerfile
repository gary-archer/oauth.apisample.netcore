#
# Build the docker image when invoked from the deployment script
# After building, files in the image can be viewed via the below commands
# - eval $(minikube docker-env --profile api)
# - docker run -it demoapi:v1 sh
#

# Use the .Net Core Debian linux docker image
# https://hub.docker.com/_/microsoft-dotnet-runtime
FROM mcr.microsoft.com/dotnet/runtime:5.0

# Install curl for troubleshooting purposes
RUN apt-get update
RUN apt-get install curl -y
RUN apt-get install openssl -y

# Set the API folder
WORKDIR /usr/api

# Copy libraries and other files into our docker image
COPY bin/Release/netcoreapp5/linux-x64/publish/*  /usr/api/
COPY kubernetes/api.config.json                   /usr/api/
COPY data/*                                       /usr/api/data/

# Create a low privilege user, which will by default have read access to our files under /usr
RUN adduser --disabled-password --home /home/apiuser --gecos '' apiuser

# Configure the Linux OS to trust the Cluster root certificate, to enable HTTPS calls inside the cluster
# An example is calling a Kubernetes deployed Authorization Server at https://login.mycompany.com
# Note that Debian's update-ca-certificates requires files to have a CRT extension
COPY certs/kubernetes/internal/svc.default.cluster.local.ca.pem /usr/local/share/ca-certificates/trusted.ca.crt
RUN update-ca-certificates

# When a container is run with this image, run the API as the low privilege user
USER apiuser
CMD ["dotnet", "sampleapi.dll"]

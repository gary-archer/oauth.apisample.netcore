#
# Build the docker image when invoked from the deployment script
#

# Use the .Net Core Debian linux docker image
# https://hub.docker.com/_/microsoft-dotnet-runtime
FROM mcr.microsoft.com/dotnet/runtime:5.0

# Install curl for troubleshooting purposes
RUN apt-get update
RUN apt-get install curl -y
RUN apt-get install openssl -y

# Set the API folder
WORKDIR /usr/sampleapi

# Copy libraries and other files into our docker image
COPY bin/Release/netcoreapp5/linux-x64/publish/*  /usr/sampleapi/
COPY deployment/api.config.json                   /usr/sampleapi/
COPY data/*                                       /usr/sampleapi/data/

# Create a low privilege user, which will by default have read access to our files under /usr
RUN adduser --disabled-password --home /home/sampleuser --gecos '' sampleuser

# Configure the Linux OS to trust development root certificates
# Note that Debian's update-ca-certificates requires files to have a CRT extension
COPY certs/mycompany.ca.pem     /usr/local/share/ca-certificates/mycompany.ca.crt
COPY deployment/charlesroot.pem /usr/local/share/ca-certificates/charlesroot.crt
RUN update-ca-certificates

# When a container is run with this image, run the API as the low privilege user
USER sampleuser
CMD ["dotnet", "sampleapi.dll"]

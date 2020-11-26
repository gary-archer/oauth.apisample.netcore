#
# Build the docker image when invoked from the deployment script
#

#
# TODO: Work out problems with Debian listed here
# Disable curl by default
# Follow file publishing best practice
# https://medium.com/01001101/containerize-your-net-core-app-the-right-way-35c267224a8d
#

# Use the .Net Core linux docker image
# https://hub.docker.com/_/microsoft-dotnet-runtime
FROM mcr.microsoft.com/dotnet/runtime:5.0

# Install curl for troubleshooting purposes
RUN apt-get update
RUN apt-get install curl -y

# Set the API folder
WORKDIR /usr/sampleapi

# Copy libraries and other files into our docker image
COPY bin/Release/netcoreapp5/linux-musl-x64/publish/*  /usr/sampleapi/
COPY deployment/api.config.json                        /usr/sampleapi/
COPY data/*                                            /usr/sampleapi/data/

# Create a low privilege user, which will by default have read access to our files under /usr
RUN addgroup -gid 1001 samplegroup
RUN adduser -u 1001 -gid 1001 -h /home/sampleuser -D sampleuser

# TODO: Get this working
RUN adduser -u 1001 -gid 1001 -h /home/sampleuser sampleuser

# Configure the Linux OS to trust the Charles Proxy Root Certificate from the Developer PC
#COPY deployment/charlesroot.pem /usr/local/share/ca-certificates
#RUN update-ca-certificates

# When a container is run with this image, run the API as the low privilege user
#USER sampleuser
CMD ["dotnet", "sampleapi.dll"]

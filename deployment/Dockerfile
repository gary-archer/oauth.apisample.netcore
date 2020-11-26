#
# Build the docker image when invoked from the deployment script
#

# Use the .Net Core docker image for the lightweight Alpine Linux OS
FROM mcr.microsoft.com/dotnet/core/runtime:3.1-alpine3.12

# Install curl for troubleshooting purposes
RUN apk --no-cache add curl

# Set the API folder
WORKDIR /usr/sampleapi

# Copy libraries and other files into our docker image
COPY bin/Release/netcoreapp5/linux-musl-x64/publish/*.dll         /usr/sampleapi/
COPY bin/Release/netcoreapp5/linux-musl-x64/publish/*.so          /usr/sampleapi/
COPY bin/Release/netcoreapp5/linux-musl-x64/publish/*.a           /usr/sampleapi/
COPY deployment/api.config.json                                     /usr/sampleapi/
COPY data/*                                                         /usr/sampleapi/data/

# Create a low privilege user, which will by default have read access to our files under /usr
RUN addgroup -g 1001 samplegroup
RUN adduser -u 1001 -G samplegroup -h /home/sampleuser -D sampleuser

# Configure the Linux OS to trust the Charles Proxy Root Certificate from the Developer PC
#COPY deployment/charlesroot.pem /usr/local/share/ca-certificates
#RUN update-ca-certificates

# When a container is run with this image, run the API as the low privilege user
USER sampleuser
CMD ["dotnet", "sampleapi.dll"]

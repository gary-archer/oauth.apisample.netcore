# Use the .Net Core Debian linux docker image
# https://hub.docker.com/_/microsoft-dotnet-aspnet
FROM mcr.microsoft.com/dotnet/aspnet:6.0

# Run a script from a runtime argument to do initialization of trusted certificates when required
ARG TRUSTED_CA_CERTS
COPY $TRUSTED_CA_CERTS /usr/local/share/certificates/trusted.ca.crt
COPY deployment/shared/docker-init.sh /tmp/
RUN chmod +x /tmp/docker-init.sh && /tmp/docker-init.sh

# Copy libraries and other files into our docker image
WORKDIR /usr/api
COPY bin/Release/netcoreapp6/linux-x64/publish/*  /usr/api/
COPY data/*                                       /usr/api/data/

# Run the app as a low privilege user
RUN adduser --disabled-password --home /home/apiuser --gecos '' apiuser
USER apiuser
CMD ["dotnet", "sampleapi.dll"]

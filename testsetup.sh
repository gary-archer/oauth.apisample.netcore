#!/bin/bash

##############################################################
# A script to build and run the API with a test configuration
# The script also runs Wiremock as a mock authorization server
##############################################################

cd "$(dirname "${BASH_SOURCE[0]}")"

#
# Download development SSL certificates if required
#
./downloadcerts.sh
if [ $? -ne 0 ]; then
  exit 1
fi

#
# Copy down the test configuration, to point the API to Wiremock rather than AWS Cognito
#
cp deployment/environments/test/api.config.json ./api.config.json

#
# Build the API every time
#
./build.sh
if [ $? -ne 0 ]; then
  exit 1
fi

#
# Get the platform
#
case "$(uname -s)" in

  Darwin)
    PLATFORM="MACOS"
 	;;

  MINGW64*)
    PLATFORM="WINDOWS"
	;;

  Linux)
    PLATFORM="LINUX"
	;;
esac

#
# Run the API and Wiremock in child windows
#
echo 'Running API ...'
if [ "$PLATFORM" == 'MACOS' ]; then
  
  open -a Terminal ./run_wiremock.sh
  open -a Terminal ./run_api.sh

elif [ "$PLATFORM" == 'WINDOWS' ]; then
  
  GIT_BASH='C:\Program Files\Git\git-bash.exe'
  "$GIT_BASH" -c ./run_wiremock.sh &
  "$GIT_BASH" -c ./run_api.sh &

elif [ "$PLATFORM" == 'LINUX' ]; then

  gnome-terminal -- ./run_wiremock.sh
  gnome-terminal -- ./run_api.sh

fi

#
# Wait for the API to become available
#
echo 'Waiting for API endpoints to come up ...'
API_URL='https://apilocal.authsamples-dev.com:446/investments/companies'
while [ "$(curl -k -s -X GET -o /dev/null -w '%{http_code}' "$API_URL")" != '401' ]; do
  sleep 2
done

#
# Wait for Wiremock endpoints to become available
#
echo 'Waiting for Wiremock endpoints to come up ...'
WIREMOCK_URL='https://login.authsamples-dev.com:447/__admin/mappings'
while [ "$(curl -k -s -X GET -o /dev/null -w '%{http_code}' "$WIREMOCK_URL")" != '200' ]; do
  sleep 2
done

#
# Restore the development configuration once the API is loaded
#
cp deployment/environments/dev/api.config.json ./api.config.json

#
# Indicate success
#
echo "Start tests via './integration_tests.sh' or './load_test.sh' ..."

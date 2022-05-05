#!/bin/bash

#####################################
# A script to run the basic load test
#####################################

cd "$(dirname "${BASH_SOURCE[0]}")"
cd test

#
# Run tests with this category and with output verbosity that includes test names
#
dotnet test --filter Category="Load" -l "console;verbosity=normal"

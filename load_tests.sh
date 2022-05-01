#!/bin/bash

######################################################
# A script to run integration tests and output results
######################################################

cd "$(dirname "${BASH_SOURCE[0]}")"
cd test

#
# Run tests with this category and with output verbosity that includes test names
#
dotnet test --filter TestCategory="Load"
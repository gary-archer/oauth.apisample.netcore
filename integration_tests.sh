#!/bin/bash

######################################################
# A script to run integration tests and output results
######################################################

cd "$(dirname "${BASH_SOURCE[0]}")"
cd test

#
# Run tests with this category and with output verbosity that includes test names
# https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test?tabs=dotnet-test-with-vstest
#
dotnet test --filter Category="Integration" --logger "console;verbosity=normal" --tl:off

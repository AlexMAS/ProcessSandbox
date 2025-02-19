#!/bin/bash

set -e

cd $(dirname "$0")

docker run --rm -v $PWD:/src gcc:12.4.0-bookworm g++ -static /src/sandbox-exec.cpp -o /src/sandbox-exec

chmod +x sandbox-exec

cd - > /dev/null

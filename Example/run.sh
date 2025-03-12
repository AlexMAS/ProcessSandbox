#!/bin/bash

set -e

cd $(dirname "$0")

exampleImage=process-sandbox-example:latest
docker build --file build.Dockerfile -t $exampleImage .
clear

echo $'\nSimple'
docker run --rm $exampleImage ./Example ls .

echo $'\nCPU Load'
docker run --rm $exampleImage ./Example dd if=/dev/zero of=/dev/null bs=4096 count=1000000

cd - > /dev/null

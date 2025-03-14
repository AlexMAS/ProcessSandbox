#!/bin/bash

cd $(dirname "$0")

rm -rf ProcessSandbox/bin/publish/ 2> /dev/null
rm -rf ProcessSandbox.App/bin/publish/ 2> /dev/null

mkdir -p ProcessSandbox/bin/publish/
mkdir -p ProcessSandbox.App/bin/publish/

set -e

docker build --file build.Dockerfile -t sandbox:latest .
id=$(docker create sandbox:latest)
docker cp $id:/bin/ProcessSandbox/. ProcessSandbox/bin/publish/
docker cp $id:/bin/ProcessSandbox.App/. ProcessSandbox.App/bin/publish/
docker rm -v $id

cd - > /dev/null

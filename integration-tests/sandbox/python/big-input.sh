#!/bin/bash

export SANDBOX_TOTAL_TIMEOUT=3000
export SANDBOX_CPU_LIMIT=1000
export SANDBOX_MEMORY_LIMIT=10000000
export SANDBOX_STDOUT_LIMIT=-1
export SANDBOX_STDERR_LIMIT=-1
export INPUT_FILE=/tmp/tests/$APP/$LANG/$TEST.in

mkdir -p $(dirname $INPUT_FILE)
dd status=none if=/dev/zero of=$INPUT_FILE bs=1M count=10

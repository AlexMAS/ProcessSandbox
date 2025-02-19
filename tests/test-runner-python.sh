#!/bin/bash

export WORKING_DIR=/out/$APP/$LANG/$TEST
export TEST_EXE="python3 /tests/$APP/$LANG/$TEST.py"

mkdir -p $WORKING_DIR

/tests/$APP/test-runner.sh $1

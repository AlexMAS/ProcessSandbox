#!/bin/bash

export WORKING_DIR=/out/$APP/$LANG/$TEST
export TEST_EXE="$WORKING_DIR/$TEST.exe"

mkdir -p $WORKING_DIR
g++ /tests/$APP/$LANG/$TEST.cpp -o $TEST_EXE

/tests/$APP/test-runner.sh $1

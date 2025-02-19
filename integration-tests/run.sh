#!/bin/bash

cd $(dirname "$0")

rm -rf ../integration-tests-out/ 2> /dev/null

test=${1:-*}

chmod +x *.sh
chmod +x */*.sh

declare -A images
images[cpp]=gcc:13.1.0-bookworm
images[python]=python:3.11.3-bullseye

echo Run test cases

for a in */ ; do
    app=$(basename $a)
    echo "  $app"

    for l in $a/*/ ; do
        lang=$(basename $l)
        langImage=${images[$lang]}
        echo "    |- $lang [ $langImage ]"

        for t in $l/*$test*.sh; do
            testFile="$(basename "$t")"
            testName="${testFile%.*}"
            echo "    |    |- $testName"

            runnerId=$(docker create \
                    --init \
                    --env APP=$app \
                    --env LANG=$lang \
                    --env TEST=$testName \
                    -v $PWD/../ProcessSandbox.App/bin/publish/sandbox:/usr/bin/sandbox \
                    -v $PWD/../ProcessSandbox.App/bin/publish/sandbox-exec:/usr/bin/sandbox-exec \
                    -v $PWD/:/tests:ro \
                    $langImage \
                    bash -c "/tests/test-runner-\$LANG.sh /tests/\$APP/\$LANG/\$TEST.sh")

            docker start --attach $runnerId
            docker cp -q $runnerId:/out/. ../integration-tests-out/
            docker rm -v $runnerId 1> /dev/null
        done
    done
done

cd - > /dev/null

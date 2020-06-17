#!/bin/sh

BUILD_SCRIPT="./node_modules/upm-template-utils/index.js"

if [ ! -f $BUILD_SCRIPT ]; then
	npm install upm-template-utils --registry https://api.bintray.com/npm/unity/unity-staging --loglevel error >/dev/null
fi

if [ -z $1 ]; then
	node $BUILD_SCRIPT
else
	node $BUILD_SCRIPT $*
fi

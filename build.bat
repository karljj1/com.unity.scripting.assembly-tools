@ECHO OFF

REM Wrapper script to make running the node.js CI script a little quicker.
REM Arguments are same as for index.js

SET BUILD_SCRIPT="./node_modules/upm-template-utils/index.js"

IF NOT EXIST %BUILD_SCRIPT% (
	CALL npm install upm-template-utils --registry https://api.bintray.com/npm/unity/unity-staging --loglevel error >NUL
)

IF "%1" == "" (
	node %BUILD_SCRIPT%
) ELSE (
	node %BUILD_SCRIPT% %*
)

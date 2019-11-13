#! /bin/sh

echo "Attempting to run tests."
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
    -batchmode \
    -nographics \
    -username '$UNITY_USERNAME' \
    -password '$UNITY_PASSWORD' \
    -runTests \
    -testPlatform playmode \
    -projectPath $(pwd) \
    -testResults $(pwd)/results.xml \
    -logFile -

cat $(pwd)/results.xml

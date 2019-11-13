#! /bin/sh

echo "Attempting to run tests."
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
    -batchmode \
    -nographics \
    -runTests \
    -testPlatform playmode \
    -projectPath $(pwd) \
    -testResults $(pwd)/results.xml \
    -logFile -

cat $(pwd)/results.xml

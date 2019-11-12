#! /bin/sh

echo "Attempting to run tests."
/Applications/Unity/Unity.app/Contents/MacOS/Unity 
    -runTests 
    -testPlatform playmode 
    -projectPath $(pwd) 
    -testResults $(pwd)/results.xml 
    -logFile $(pwd)/unity.log

cat $(pwd)/results.xml

cat $(pwd)/unity.log

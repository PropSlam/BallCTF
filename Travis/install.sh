#! /bin/sh

VERSION=2019.2.11f1

curl -o unity.zip $UNITY_EDITOR_URL
unzip unity.zip

sudo installer -dumplog -package Unity.pkg -target /
sudo installer -dumplog -package UnitySetup-Linux-Support-for-Editor-$VERSION.pkg -target /
sudo installer -dumplog -package UnitySetup-Mac-IL2CPP-Support-for-Editor-$VERSION.pkg -target /
sudo installer -dumplog -package UnitySetup-Windows-Mono-Support-for-Editor-$VERSION.pkg -target /

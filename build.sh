#!/usr/bin/env bash

cp README.md FCAAddon/DOCS.md
cp README.md FCAAddon/.

VERSION=$(cat FCAAddon/config.yaml| grep version | grep -P -o "[\d\.]*")

echo git tag -a $VERSION -m $VERSION

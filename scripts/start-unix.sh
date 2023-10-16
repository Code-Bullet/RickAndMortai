#!/bin/bash
set -ex

# Script to start the RickAndMorty project on macOS and Linux.
# Loads environment variables (OpenAI keys) from `.env`

source .env

launchctl setenv OPENAI_API_KEY $OPENAI_API_KEY
launchctl setenv FAKE_YOU_USERNAME_OR_EMAIL $FAKE_YOU_USERNAME_OR_EMAIL
launchctl setenv FAKE_YOU_PASSWORD $FAKE_YOU_PASSWORD

# /Applications/Unity/Hub/Editor/2022.3.11f1/Unity.app/Contents/MacOS/Unity -projectPath .
/Applications/Unity\ Hub.app/Contents/MacOS/Unity\ Hub -projectPath .
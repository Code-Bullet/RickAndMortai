#!/bin/bash
set -ex

source .env
# export RAM_SCENE_ID="scene-c6a858ce-7261-88ed-b205-5eac1e59a125"
export RAM_SCENE_ID="scene-96df313a-692c-406a-3007-5a86bbe28c95"
export RAM_SCENE_ID="scene-b51dead6-948b-bac9-5885-32065b30f749"

# export RAM_SCENE_ID="scene-0eb7fec2-e9ed-5a25-ebe8-771a38c2dcff"
./app.app/Contents/MacOS/Rick\ and\ Morty\ Chatgpt -screen-fullscreen 0 -logFile buildLog.txt

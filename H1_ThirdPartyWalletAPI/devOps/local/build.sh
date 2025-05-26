#!/bin/bash

set -x

PROJECT_SCRIPTS_PATH=$(dirname $0)
cd ${PROJECT_SCRIPTS_PATH}

# 專案腳本目錄
PROJECT_SCRIPTS_PATH=$(pwd)

cd ${PROJECT_SCRIPTS_PATH}/../..

# 專案跟目錄
PROJECT_ROOT_PATH=$(pwd)

# 載入全域環境變數
. ${PROJECT_SCRIPTS_PATH}/env.sh

docker build \
    --tag="${PROJECT_IMAGE_TAG}"                                    \
    --compress                                                      \
    --force-rm                                                      \
    --no-cache                                                      \
    .
    
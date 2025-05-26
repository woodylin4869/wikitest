#!/bin/bash

set -x

PROJECT_SCRIPTS_PATH=$(dirname $0)
cd ${PROJECT_SCRIPTS_PATH}

# 專案腳本目錄
PROJECT_SCRIPTS_PATH=$(pwd)

cd ${PROJECT_SCRIPTS_PATH}/../..

# 專案根目錄
export PROJECT_ROOT_PATH=$(pwd)

# 載入全域環境變數
. ${PROJECT_SCRIPTS_PATH}/env.sh

# 把存在的相關資源移除, 包含容器, 網路
${PROJECT_SCRIPTS_PATH}/reset.sh

# 建立測試用網路
docker network create ${NETWORK_NAME}

# 建立儲存體服務
${PROJECT_SCRIPTS_PATH}/storages.sh

TESTED_CONTAINER_NAME=${DOCKER_PREFIX}_target
TESTED_API_PORT=80

docker run                                                      \
    -it                                                         \
    --network=${NETWORK_NAME}                                   \
    --env=STORAGE_ROOT_PATH="/tmp/jmeter"                       \
    --env=TESTED_CONTAINER_NAME=${TESTED_CONTAINER_NAME}        \
    --env=TESTED_API_PORT=${TESTED_API_PORT}                    \
    --env=PROJECT_NAME=${PROJECT_NAME}                          \
    --env=TEST_FILE=devOps/jmeter/test.jmx                      \
    --env=LOGIN_USER_NAME=${LOGIN_USER_NAME}                    \
    --env=LOGIN_USER_ID=${LOGIN_USER_ID}                        \
    --env=LOGIN_GROUP_NAME=${LOGIN_GROUP_NAME}                  \
    --env=LOGIN_GROUP_ID=${LOGIN_GROUP_ID}                      \
    --rm                                                        \
    --volume="${PWD}:/project"                                  \
    --workdir=/project/devOps/container_jmeter                  \
    --entrypoint=""                                             \
    ${JMETER_IMAGE_TAG}                                         \
    /bin/ash

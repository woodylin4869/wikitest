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

# 建立受測容器
docker run \
    -d \
    --name=${TESTED_CONTAINER_NAME} \
    --rm \
    --publish=8888:80 \
    --log-driver local \
    --network=${NETWORK_NAME} \
    --env=DOTNET_ENVIRONMENT=test \
    --volume=${PROJECT_ROOT_PATH}/appsettings.test.json:/app/appsettings.test.json \
    --volume=${PROJECT_ROOT_PATH}/DockerBuild/docker-entrypoint.sh:/usr/local/sbin/docker-entrypoint.sh \
    ${PROJECT_IMAGE_TAG}

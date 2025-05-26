## !/bin/bash
set -x

if [ "${DOCKER_PREFIX}" == "" ]; then
	echo "環境變數 DOCKER_PREFIX 未設定"
	exit 1
fi

if [ "${PROJECT_IMAGE_TAG}" == "" ]; then
	echo "環境變數 PROJECT_IMAGE_TAG 未設定"
	exit 1
fi

# 清除容器
CONTAINERS=$(docker ps --filter="name=${DOCKER_PREFIX}_*" -aq)
if [ "${CONTAINERS}" != "" ]; then
	docker rm -f $CONTAINERS
fi

# 清除網路
NETWORKS=$(docker network ls --filter="name=${DOCKER_PREFIX}_*" -q)
if [ "${NETWORKS}" != "" ]; then
	docker network rm ${NETWORKS}
fi

IMAGE_NAME=$(docker image ls | grep h1_thirdparty_wallet)

if [ "${IMAGE_NAME}" != "" ]; then
	docker image rm ${PROJECT_IMAGE_TAG}
fi

docker images
docker image prune -f
docker images

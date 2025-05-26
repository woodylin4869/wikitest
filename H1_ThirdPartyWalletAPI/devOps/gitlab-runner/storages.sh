## !/bin/bash

set -x

if [ "${PROJECT_ROOT_PATH}" == "" ]; then
	echo "環境變數 PROJECT_ROOT_PATH 未設定"
	exit 1
fi

if [ "${DOCKER_PREFIX}" == "" ]; then
	echo "環境變數 DOCKER_PREFIX 未設定"
	exit 1
fi

if [ "${NETWORK_NAME}" == "" ]; then
	echo "環境變數 NETWORK_NAME 未設定"
	exit 1
fi

# 執行 redis 容器
docker run                                                  \
    -d                                                      \
    --name=${DOCKER_PREFIX}_redis                           \
    --rm                                                    \
    --network=${NETWORK_NAME}                               \
    --env=REDIS_PASSWORD=password                           \
    redis:5.0.4-alpine

# 執行 pgsql 容器
# [configuration](https://github.com/bitnami/containers/tree/main/bitnami/postgresql#configuration)
docker run                                                      \
    -d                                                          \
    --name=${DOCKER_PREFIX}_pgsql                               \
    --rm                                                        \
    --network=${NETWORK_NAME}                                   \
    --publish="4321:5432"                                       \
    --env=POSTGRES_PASSWORD=password                            \
    --volume=${PROJECT_ROOT_PATH}/devOps/docker-volumes/pgsql/docker-entrypoint-initdb.d:/docker-entrypoint-initdb.d \
    bitnami/postgresql:14.6.0
#/docker-entrypoint-initdb.d

#docker cp $PGSQL_DATA ${EXT_PREFIX}_pgsql:/docker-entrypoint-initdb.d

#    --env=POSTGRESQL_USERNAME=cwapi                             \
#    --env=POSTGRESQL_DATABASE=centerwallet                      \

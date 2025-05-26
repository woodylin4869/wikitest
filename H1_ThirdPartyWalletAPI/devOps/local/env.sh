#!/bin/bash

# 專案名稱
PROJECT_NAME=ThirdPartyWallet

# 映像檔標籤
PROJECT_IMAGE_TAG=h1_thirdparty_wallet

# 容器環境前置名稱
DOCKER_PREFIX=D_888

# 網路名稱
NETWORK_NAME=${DOCKER_PREFIX}_network


JMETER_IMAGE_TAG=justb4/jmeter:5.5


LOGIN_USER_NAME=$(id -gn)
LOGIN_USER_ID=$(id -u)
LOGIN_GROUP_NAME=$(id -gn)
LOGIN_GROUP_ID=$(id -g)


export  PROJECT_NAME \
        PROJECT_IMAGE_TAG \
        DOCKER_PREFIX \
        NETWORK_NAME \
        JMETER_IMAGE_TAG \
        LOGIN_USER_NAME \
        LOGIN_USER_ID \
        LOGIN_GROUP_NAME \
        LOGIN_GROUP_ID


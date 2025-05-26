#!/bin/ash

# 初始化執行環境, 在容器內建立操作人員帳號
# 在 runner 下需要建立帳號. 但須安裝必要套件
set -e

if [ "${LOGIN_USER_NAME}" == "" ]; then
	echo "環境變數 LOGIN_USER_NAME 未設定"
	exit 1
fi
if [ "${LOGIN_USER_ID}" == "" ]; then
	echo "環境變數 LOGIN_USER_ID 未設定"
	exit 1
fi
if [ "${LOGIN_GROUP_NAME}" == "" ]; then
	echo "環境變數 LOGIN_GROUP_NAME 未設定"
	exit 1
fi
if [ "${LOGIN_GROUP_ID}" == "" ]; then
	echo "環境變數 LOGIN_GROUP_ID 未設定"
	exit 1
fi

apk add libxml2-utils shadow

exit 0

LOCAL_USER_ID=$(awk -F: '$3 == "999" {print $1}' /etc/passwd)
LOCAL_GROUP_ID=$(awk -F: '$3 == "999" {print $1}' /etc/group)
if [ "${LOCAL_USER_ID}" == "" ] && [ "${LOCAL_GROUP_ID}" == "" ]; then
	# 完全不存在
	if  [ ! -z "${LOGIN_USER_NAME}" ]; then
		if [ -z $(getent passwd ${LOGIN_USER_NAME}) ]; then
			ARGS=""
			if  [ -d "/home/${LOGIN_USER_NAME}" ]; then
				ARGS="-H"
			fi
			adduser ${ARGS} -h "/home/${LOGIN_USER_NAME}" -D -u ${LOGIN_USER_ID} ${LOGIN_USER_NAME} ${LOGIN_GROUP_NAME}
		fi
	fi
	exit 0
fi

if [ "${LOCAL_USER_ID}" == "" ] && [ "${LOCAL_GROUP_ID}" != "" ]; then
	# 使用者不存在, 群組存在
	if  [ ! -z "${LOGIN_USER_NAME}" ]; then
		if [ -z $(getent passwd ${LOGIN_USER_NAME}) ]; then
			ARGS=""
			if  [ -d "/home/${LOGIN_USER_NAME}" ]; then
				ARGS="-H"
			fi
			adduser ${ARGS} -h "/home/${LOGIN_USER_NAME}" -D -u ${LOGIN_USER_ID} ${LOGIN_USER_NAME} ${LOGIN_GROUP_NAME}
		fi
	fi
	exit 0
fi


#!/bin/ash

set -x

./init.sh

if [ "${?}" != "0" ]; then
	echo "環境初始化錯誤!!"
	exit 1
fi


if [ "${TESTED_CONTAINER_NAME}" == "" ]; then
	echo "環境變數 TESTED_CONTAINER_NAME 未設定"
	exit 1
fi
if [ "${TESTED_API_PORT}" == "" ]; then
	echo "環境變數 TESTED_API_PORT 未設定"
	exit 1
fi
if [ "${TESTED_API_PORT}" == "" ]; then
	echo "環境變數 TESTED_API_PORT 未設定"
	exit 1
fi
if [ "${STORAGE_ROOT_PATH}" == "" ]; then
	echo "環境變數 STORAGE_ROOT_PATH 未設定"
	exit 1
fi
if [ "${PROJECT_NAME}" == "" ]; then
	echo "環境變數 PROJECT_NAME 未設定"
	exit 1
fi
if [ "${TEST_FILE}" == "" ]; then
	echo "環境變數 TEST_FILE 未設定"
	exit 1
fi

PROJECT_SCRIPTS_PATH=$(dirname $0)
cd ${PROJECT_SCRIPTS_PATH}

# 專案腳本目錄
PROJECT_SCRIPTS_PATH=$(pwd)

cd ${PROJECT_SCRIPTS_PATH}/../..

# 專案根目錄
export PROJECT_ROOT_PATH=$(pwd)


REPORT_DIR=${STORAGE_ROOT_PATH}/report
JMETER_LOG=${STORAGE_ROOT_PATH}/${PROJECT_NAME}.log
TEST_LOG=${STORAGE_ROOT_PATH}/${PROJECT_NAME}.xml
PROJECT_REPORTS="${PROJECT_ROOT_PATH}/reports"

rm -rf ${STORAGE_ROOT_PATH} > /dev/null 2>&1
mkdir -p ${STORAGE_ROOT_PATH}

jmeter \
    --systemproperty=log4j2.formatMsgNoLookups=true \
    --systemproperty=log_level.jmeter=DEBUG \
    --jmeterproperty=targetHost=${TESTED_CONTAINER_NAME} \
    --jmeterproperty=targetPort=${TESTED_API_PORT} \
    --jmeterproperty=viewResultsTreeFilename=${TEST_LOG} \
    --forceDeleteResultFile \
    --nongui \
    --testfile=${TEST_FILE} \
    --jmeterlogfile=${JMETER_LOG} \
    --reportoutputfolder ${REPORT_DIR}

mkdir -p ${PROJECT_REPORTS} > /dev/null 2>&1
chown -R ${LOGIN_USER_ID}:${LOGIN_GROUP_ID} ${PROJECT_REPORTS}

chown -R ${LOGIN_USER_ID}:${LOGIN_GROUP_ID} ${STORAGE_ROOT_PATH}
cp -a ${STORAGE_ROOT_PATH} ${PROJECT_REPORTS}

XML_RESULT=$(xmllint --xpath "count(/testResults/httpSample/assertionResult[failure=\"true\"]|/testResults/httpSample/assertionResult[error=\"true\"])" ${TEST_LOG})
echo "例外數量:${XML_RESULT}"

if [ "${XML_RESULT}" == "0" ]; then
    echo "驗證成功"
	exit 0
fi
echo "驗證失敗"
exit 1

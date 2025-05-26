# 使用說明


### 目錄架構

![目錄架構](/devOps/images/001.png)

- devOps/gitlab-runner
gitlab-cl runner 要執行的腳本, 通常會在 .gitlab-ci.yml 內部引用.

- devOps/docker-volumes
存放容器掛載資料的根目錄, 譬如 pgsql 的初始化腳本.
	- devOps/docker-volumes/pgsql/docker-entrypoint-initdb.d
這裡放測試時需要的基本資料, 隨著測例的擴充, 這邊的資料也會擴充.

- devOps/container_jmeter
存放自動化測試容器執行的腳本, 提供給 devOps/gitlab-runner/run.sh 或 devOps/local/run.sh 呼叫.

- devOps/jmeter
自動化測試/壓測的規則腳本, 需要依據專案進行維護修改. [編輯自動化測試腳本工具](https://jmeter.apache.org/download_jmeter.cgi)

- devOps/local
在開發端執行的腳本, 提供給開發者進行自動化驗證. 完成後才 push 到 dev 進行 gitlab 的自動化驗證.

- DockerBuild
docker build 專案容器所需要的外部資源

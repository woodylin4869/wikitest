--
-- 初始化 centerwallet 資料庫
--

-- 建立 admin 帳號
CREATE ROLE "admin" CREATEDB LOGIN PASSWORD 'password';
GRANT "pg_read_all_data" TO "admin" WITH ADMIN OPTION;
GRANT Connect, Create, Temporary ON DATABASE "postgres" TO "admin";
GRANT Create, Usage ON SCHEMA "public" TO "admin";

-- 建立 api_group 群組
CREATE ROLE "api_group";
GRANT "pg_execute_server_program" TO "api_group";
GRANT "pg_read_all_data" TO "api_group";
GRANT "pg_write_all_data" TO "api_group";

-- 建立 partition_manager 帳號
CREATE ROLE "partition_manager";

-- 建立 schedule_api 帳號
CREATE ROLE "schedule_api" LOGIN PASSWORD 'password';
GRANT "api_group" TO "schedule_api";
GRANT "partition_manager" TO "schedule_api";

-- 建立 wash_order_count_api 帳號
CREATE ROLE "wash_order_count_api" LOGIN PASSWORD 'password';
GRANT "api_group" TO "wash_order_count_api";

-- 建立 cwapi 帳號
CREATE ROLE "cwapi" LOGIN PASSWORD 'password';
GRANT "api_group" TO "cwapi";


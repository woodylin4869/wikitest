\c centerwallet;

CREATE TABLE "public"."t_admin" (
  "user_account" varchar(20) COLLATE "pg_catalog"."default" NOT NULL,
  "user_password" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "role" varchar(10) COLLATE "pg_catalog"."default" NOT NULL
)
;

ALTER TABLE "public"."t_admin" OWNER TO "cwapi";

ALTER TABLE "public"."t_admin" ADD PRIMARY KEY ("user_account");

-- user_password = password
INSERT INTO "public"."t_admin"("user_account", "user_password", "role") VALUES ('admin001', '$2a$11$dToyUEwvEJNlTzBDmpWoXOkD7tVdElUI/UbQKHDamXkRF8kC26UVS', 'admin');

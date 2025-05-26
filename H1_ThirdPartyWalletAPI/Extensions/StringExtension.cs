using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace H1_ThirdPartyWalletAPI.Extensions
{
    public static class StringExtension
    {
        /// <summary>将大驼峰命名转为小驼峰命名</summary>
        public static string RenameCamelCase(this string str)
        {
            var firstChar = str[0];

            if (firstChar == char.ToLowerInvariant(firstChar))
            {
                return str;
            }

            var name = str.ToCharArray();
            name[0] = char.ToLowerInvariant(firstChar);

            return new String(name);
        }

        /// <summary>将大驼峰命名转为蛇形命名</summary>
        public static string RenameSnakeCase(this string str)
        {
            var builder = new StringBuilder();
            var name = str;
            var previousUpper = false;

            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];
                if (char.IsUpper(c))
                {
                    if (i > 0 && !previousUpper)
                    {
                        builder.Append("_");
                    }
                    builder.Append(char.ToLowerInvariant(c));
                    previousUpper = true;
                }
                else
                {
                    builder.Append(c);
                    previousUpper = false;
                }
            }
            return builder.ToString();
        }

        /// <summary>轉小寫忽略蛇行字元(_)</summary>
        public static string ToLowerAndIgnoreSnakeCase(this string str)
        {
            return str.ToLower().Replace("_", "");
        }

        /// <summary>
        /// 回傳該值，如果是Null或是Empty回傳Null
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string? StringOrDefault(this string str)
        {
            return string.IsNullOrEmpty(str) ? null : str;
        }

        public static bool IsJson(this string source)
        {
            if (source == null)
                return false;

            try
            {
                JsonDocument.Parse(source);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// 驗證是否為yyyyMMdd日期格式 回傳 true:合法 false:非法
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsDatetimeFormat(this string value)
        {
            DateTime _date;
            if (!DateTime.TryParseExact(value, "yyyyMMdd", null, DateTimeStyles.None, out _date))
            {
                return false;
            }
            return true;
        }
    }

}
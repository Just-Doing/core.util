using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Util.Core
{
    /// <summary>
    /// 系统扩展 - 类型转换
    /// </summary>
    public static partial class Extensions {
        /// <summary>
        /// 安全转换为字符串，去除两端空格，当值为null时返回""
        /// </summary>
        /// <param name="input">输入值</param>
        public static string SafeString( this object input ) {
            return input?.ToString().Trim() ?? string.Empty;
        }

        /// <summary>
        /// 转换为bool
        /// </summary>
        /// <param name="obj">数据</param>
        public static bool ToBool( this string obj ) {
            return Util.Core.Helpers.Convert.ToBool( obj );
        }

        /// <summary>
        /// 转换为可空bool
        /// </summary>
        /// <param name="obj">数据</param>
        public static bool? ToBoolOrNull( this string obj ) {
            return Util.Core.Helpers.Convert.ToBoolOrNull( obj );
        }

        /// <summary>
        /// 转换为int
        /// </summary>
        /// <param name="obj">数据</param>
        public static int ToInt( this string obj ) {
            return Util.Core.Helpers.Convert.ToInt( obj );
        }

        /// <summary>
        /// 转换为可空int
        /// </summary>
        /// <param name="obj">数据</param>
        public static int? ToIntOrNull( this string obj ) {
            return Util.Core.Helpers.Convert.ToIntOrNull( obj );
        }

        /// <summary>
        /// 转换为long
        /// </summary>
        /// <param name="obj">数据</param>
        public static long ToLong( this string obj ) {
            return Util.Core.Helpers.Convert.ToLong( obj );
        }

        /// <summary>
        /// 转换为可空long
        /// </summary>
        /// <param name="obj">数据</param>
        public static long? ToLongOrNull( this string obj ) {
            return Util.Core.Helpers.Convert.ToLongOrNull( obj );
        }

        /// <summary>
        /// 转换为double
        /// </summary>
        /// <param name="obj">数据</param>
        public static double ToDouble( this string obj ) {
            return Util.Core.Helpers.Convert.ToDouble( obj );
        }

        /// <summary>
        /// 转换为可空double
        /// </summary>
        /// <param name="obj">数据</param>
        public static double? ToDoubleOrNull( this string obj ) {
            return Util.Core.Helpers.Convert.ToDoubleOrNull( obj );
        }

        /// <summary>
        /// 转换为decimal
        /// </summary>
        /// <param name="obj">数据</param>
        public static decimal ToDecimal( this string obj ) {
            return Util.Core.Helpers.Convert.ToDecimal( obj );
        }

        /// <summary>
        /// 转换为可空decimal
        /// </summary>
        /// <param name="obj">数据</param>
        public static decimal? ToDecimalOrNull( this string obj ) {
            return Util.Core.Helpers.Convert.ToDecimalOrNull( obj );
        }

        /// <summary>
        /// 转换为日期
        /// </summary>
        /// <param name="obj">数据</param>
        public static DateTime ToDate( this string obj ) {
            return Util.Core.Helpers.Convert.ToDate( obj );
        }

        /// <summary>
        /// 转换为可空日期
        /// </summary>
        /// <param name="obj">数据</param>
        public static DateTime? ToDateOrNull( this string obj ) {
            return Util.Core.Helpers.Convert.ToDateOrNull( obj );
        }

        /// <summary>
        /// 转换为Guid
        /// </summary>
        /// <param name="obj">数据</param>
        public static Guid ToGuid( this string obj ) {
            return Util.Core.Helpers.Convert.ToGuid( obj );
        }

        /// <summary>
        /// 转换为可空Guid
        /// </summary>
        /// <param name="obj">数据</param>
        public static Guid? ToGuidOrNull( this string obj ) {
            return Util.Core.Helpers.Convert.ToGuidOrNull( obj );
        }

        /// <summary>
        /// 转换为Guid集合
        /// </summary>
        /// <param name="obj">数据,范例: "83B0233C-A24F-49FD-8083-1337209EBC9A,EAB523C6-2FE7-47BE-89D5-C6D440C3033A"</param>
        public static List<Guid> ToGuidList( this string obj ) {
            return Util.Core.Helpers.Convert.ToGuidList( obj );
        }

        /// <summary>
        /// 转换为Guid集合
        /// </summary>
        /// <param name="obj">字符串集合</param>
        public static List<Guid> ToGuidList( this IList<string> obj ) {
            if( obj == null )
                return new List<Guid>();
            return obj.Select( t => t.ToGuid() ).ToList();
        }

        /// <summary>
        /// 转换为Base64 字符串
        /// </summary>
        /// <param name="obj">字符串</param>
        public static string ToBase64(this string obj)
        {
            byte[] b = System.Text.Encoding.Default.GetBytes(obj);
            var base64Str = Convert.ToBase64String(b);
            return base64Str;
        }

        /// <summary>
        /// 转换为MD5 字符串
        /// </summary>
        /// <param name="obj">字符串</param>
        public static string ToMd5(this string input)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
                var strResult = System.BitConverter.ToString(result);
                return strResult.Replace("-", "");
            }
        }

        /// <summary>
        /// 转换为MD5 字符串
        /// </summary>
        /// <param name="obj">字符串</param>
        public static string GetBase64(this string input)
        {
            var bat = System.Convert.FromBase64String(input);
            var str = System.Text.Encoding.UTF8.GetString(bat);
            return str;
        }


        /// <summary>
        /// 去特殊字符
        /// </summary>
        /// <param name="input">字符串</param>
        public static string DeleteSymbol(this string input)
        {
            return new Regex("[!@#$%^&*()（）【】{}￥]", RegexOptions.IgnoreCase).Replace(input, "");
        }
    }
}

using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileDownloaderSystem.Extensions
{
    /// <summary>
    /// Extension methods for String class.
    /// </summary>
    public static class StringExtensions
    {
       /// <summary>
        /// Adds a char to end of given string if it does not ends with the char.
        /// </summary>
        public static string EnsureEndsWith(this string str, char c, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (str.EndsWith(c.ToString(), comparisonType))
            {
                return str;
            }

            return str + c;
        }

        /// <summary>
        /// Adds a char to beginning of given string if it does not starts with the char.
        /// </summary>
        public static string EnsureStartsWith(this string str, char c, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (str.StartsWith(c.ToString(), comparisonType))
            {
                return str;
            }

            return c + str;
        }
    }

    public static class MvcExtensionMethods
    {
        public static string BaseUrl(this IUrlHelper helper)
        {
            var url = string.Format("{0}://{1}", helper.ActionContext.HttpContext.Request.Scheme, helper.ActionContext.HttpContext.Request.Host.ToUriComponent());
            return url;
        }

        public static string FullUrl(this IUrlHelper helper, string virtualPath)
        {
            var url = string.Format("{0}://{1}{2}", helper.ActionContext.HttpContext.Request.Scheme, helper.ActionContext.HttpContext.Request.Host.ToUriComponent(), helper.Content(virtualPath).EnsureStartsWith('/'));

            return url;
        }
    }


}

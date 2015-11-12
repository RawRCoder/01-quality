using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
    internal static class HtmlStringUtils
    {
        private static string WrapWithQuotes(this string s)
        {
            return s.Contains("\"") ? $"'{s}'" : $"\"{s}\"";
        }

        public static string WrapWithTag(this string content, string tag, Dictionary<string, string> attributes = null)
        {
            var headingTag = tag;
            if (attributes != null && attributes.Any())
            {
                headingTag += " " + string.Join(" ",
                    attributes.Select(a => $"{a.Key}=" + a.Value.WrapWithQuotes()));
            }
            return $"<{headingTag}>{content}</{tag}>";
        }

        public static string ApplyHtmlScreening(this string content)
        {
            return content.Replace("<", "&lt;").Replace(">", "&gt;").Replace("  ", "&nbsp;&nbsp;");
        }
    }
}

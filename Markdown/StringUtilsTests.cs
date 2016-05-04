using System.Collections.Generic;
using NUnit.Framework;

namespace Markdown
{
    [TestFixture]
    class StringUtilsTests
    {
        [TestCase("lol", "p", Result = "<p>lol</p>")]
        [TestCase("lol", "div", Result = "<div>lol</div>")]
        public string TestTagWrapperWithNoAttributes(string content, string tag)
        {
            return content.WrapWithTag(tag);
        }
        [TestCase("топ кек lol", "a", "href", "#", Result = "<a href=\"#\">топ кек lol</a>")]
        public string TestTagWrapperWithOneAttribute(string content, string tag, string attrName, string attrValue)
        {
            return content.WrapWithTag(tag, new Dictionary<string, string> {{attrName, attrValue}});
        }
    }
}

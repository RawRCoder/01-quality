using System;
using System.Collections.Generic;
using Markdown;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownTests
{
    [TestClass]
    public class StringUtilsTests
    {
        [TestMethod]
        public void TestTagWrapperWithNoAttributes()
        {
            var wrapped = "Content".WrapWithTag("div");
            Assert.AreEqual("<div>Content</div>", wrapped);
        }
        [TestMethod]
        public void TestTagWrapperWithOneAttribute()
        {
            var wrapped = "Content".WrapWithTag("div", new Dictionary<string, string> {{"id", "mahDiv"}});
            Assert.AreEqual("<div id=\"mahDiv\">Content</div>", wrapped);
        }
    }
}

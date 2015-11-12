using System;
using Markdown;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

namespace MarkdownTests
{
    [TestClass]
    public class ProcessorTests
    {
        private MarkdownProcessor p;
        [SetUp]
        public void TestInitialize()
        {
            p = new MarkdownProcessor();
        }

        [TestCase("Test 123", Result = "<p>Test 123</p>", TestName = "Simple test")]
        [TestCase("Test 123\r\nTest 666", Result = "<p>Test 123 Test 666</p>", TestName = "Unite paragraphs test (Windows style)")]
        [TestCase("Test 123\nTest 666", Result = "<p>Test 123 Test 666</p>", TestName = "Unite paragraphs test (Unix style)")]
        [TestCase("Test 123\rTest 666", Result = "<p>Test 123 Test 666</p>", TestName = "Unite paragraphs test (Old Mac OS style)")]
        [TestCase("Test 123\r\n\r\nTest 666", Result = "<p>Test 123</p><p>Test 666</p>", TestName = "2 Paragraphs test (Windows style")]
        [TestCase("Test 123\n\nTest 666", Result = "<p>Test 123</p><p>Test 666</p>", TestName = "2 Paragraphs test (Unix style")]
        [TestCase("Test 123\r\rTest 666", Result = "<p>Test 123</p><p>Test 666</p>", TestName = "2 Paragraphs test (Old Mac OS style")]

        [TestCase("_Test 123_", Result = "<em>Test 123</em>", TestName = "Em simple test")]
        [TestCase("__Test 123__", Result = "<strong>Test 123</strong>", TestName = "Strong simple test")]
        [TestCase("`Test 123`", Result = "<code>Test 123</code>", TestName = "Code simple test")]

        [TestCase(" _Test 123_", Result = "<p> <em>Test 123</em></p>", TestName = "Em in paragraph test")]
        [TestCase(" __Test 123__", Result = "<p> <strong>Test 123</strong></p>", TestName = "Strong in paragraph test")]
        [TestCase(" `Test 123`", Result = "<p> <code>Test 123</code></p>", TestName = "Code in paragraph test")]

        [TestCase("_Test 123_ ", Result = "<em>Test 123</em><p> </p>", TestName = "Em and a paragraph test")]
        [TestCase("__Test 123__ ", Result = "<strong>Test 123</strong><p> </p>", TestName = "Strong and a paragraph test")]
        [TestCase("`Test 123` ", Result = "<code>Test 123</code><p> </p>", TestName = "Code and a paragraph test")]

        [TestCase("_EMEMEM __STRONG__ EMEMEM_", Result = "<em>EMEMEM <strong>STRONG</strong> EMEMEM</em>", TestName = "Em in a strong tag test")]
        [TestCase("__EMEMEM _STRONG_ EMEMEM__", Result = "<strong>EMEMEM <em>STRONG</em> EMEMEM</strong>", TestName = "String in an em tag test")]
        [TestCase("`EMEMEM _EM_ EMEMEM`", Result = "<code>EMEMEM _EM_ EMEMEM</code>", TestName = "Em in a code tag test")]
        [TestCase("`EMEMEM __STRONG__ EMEMEM`", Result = "<code>EMEMEM __STRONG__ EMEMEM</code>", TestName = "Strong in a code tag test")]

        [TestCase("__lol _azaza `kek", Result = "<p>__lol _azaza `kek</p>", TestName = "Unpaired tags test 1")]
        [TestCase("`___lol azaza kek", Result = "<p>`___lol azaza kek</p>", TestName = "Unpaired tags test 2")]
        public string SimpleValueTest(string src)
        {
            return p.Process(src);
        }
    }
}

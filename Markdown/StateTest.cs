using NUnit.Framework;

namespace Markdown
{
    [TestFixture]
    class StateTest
    {
        [TestCase("Text _abc_ Text", "_", 0, Result = 5)]
        [TestCase("Text _abc_ Text", "_", 5, Result = 0)]
        [TestCase("Text _abc_ Text", "_", 6, Result = 3)]
        [TestCase("Text _abc_ Text", "_", 9, Result = 0)]
        [TestCase("Text _abc_ Text", "_", 10, Result = -1)]
        public int TextNextSubstringRelativeId(string src, string substring, int pos = 0)
        {
            var s = new MarkdownProcessorState(src) {Position = pos};
            return s.GetNextSubstringRelativeId(substring);
        }

        [TestCase("Text _abc_ Text", "_", 0, Result = 9)]
        [TestCase("Text _abc_ Text", "_", 5, Result = 4)]
        [TestCase("Text _abc_ Text", "_", 6, Result = 3)]
        [TestCase("Text _abc_ Text", "_", 9, Result = 0)]
        [TestCase("Text _abc_ Text", "_", 10, Result = -1)]
        public int TextNextSubstringEndedWithSeporatorRelativeId(string src, string substring, int pos = 0)
        {
            var s = new MarkdownProcessorState(src) { Position = pos };
            return s.GetNextSubstringEndedWithSeporatorRelativeId(substring, MarkdownProcessor.IsSeporator);
        }
    }
}

using System.IO;

namespace Markdown
{
    public class MarkdownProcessor
    {
        public string Process(string markdownText)
        {
            var state = new MardownProcessorState(markdownText);
            while (state.MakeStep()) {}
            state.FixUnclosedTags();
            return state.ToString();
        }

        public string Process(StreamReader r)
        {
            return Process(r.ReadToEnd());
        }

    }
}

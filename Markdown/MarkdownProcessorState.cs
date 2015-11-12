using System;
using System.Linq;

namespace Markdown
{
    public class MarkdownProcessorState
    {
        public bool Finished => Position >= Source.Length;
        public int Position { get; set; } = 0;
        public string Source { get; }
        public char CurrentChar => Source[Position];
        public char? PreviousChar => Position <= 0 ? null : (char?)Source[Position - 1];
        public char? NextChar => Position + 1 >= Source.Length ? null : (char?) Source[Position + 1];
        public char? CharAt(int shift) => Position + shift >= Source.Length ? null : (char?)Source[Position + shift];
        public MarkdownRootObject Root { get; } = new MarkdownRootObject();
        public MarkdownObject CurrentObject { get; set; }
        public bool Screening { get; set; } = false;
        public bool ShouldNotGenerateSubconstructions => CurrentObject is MarkdownCodeObject;
        public MarkdownTextObject CurrentTextObject
        {
            get
            {
                var textObject = CurrentObject as MarkdownTextObject;
                if (textObject != null)
                    return textObject;
                if (CurrentObject is MarkdownRootObject)
                {
                    PushParagraphWrapper();
                    PushText();
                    textObject = CurrentObject as MarkdownTextObject;
                    CloseAndPopText();
                    return textObject;
                }

                textObject = CurrentObject.Subobjects.LastOrDefault() as MarkdownTextObject;
                if (textObject != null)
                    return textObject;
                PushText();
                textObject = CurrentObject as MarkdownTextObject;
                CloseAndPopText();
                return textObject;
            }
        }


        public MarkdownProcessorState(string src)
        {
            Source = src;
            CurrentObject = Root;
        }


        public int GetNextSubstringRelativeId(string substr)
            => Source.Substring(Position).IndexOf(substr, StringComparison.InvariantCulture);
        public int GetNextSubstringEndedWithSeporatorRelativeId(string substr, Predicate<char?> isSeporator)
        {
            var pos = Position;
            while (pos < Source.Length)
            {
                var str = Source.Substring(pos);
                var id = str.IndexOf(substr, StringComparison.InvariantCulture);
                if (id < 0)
                    return id;
                if (isSeporator(CharAt(pos + id + substr.Length)))
                    return pos + id - Position;
                pos += id + substr.Length;
            }
            return -1;
        }

        public string LookForward(int symbols) => Source.Substring(Position, Math.Min(symbols, Source.Length - Position));
        private void PushText()
        {
            CloseAndPopText();
            CurrentObject = new MarkdownTextObject(CurrentObject);
        }
        public void PushStrongWrapper()
        {
            CloseAndPopText();
            CurrentObject = new MarkdownStrongObject(CurrentObject);
        }
        public void PushEmWrapper()
        {
            CloseAndPopText();
            CurrentObject = new MarkdownEmObject(CurrentObject);
        }
        public void PushCodeWrapper()
        {
            CloseAndPopText();
            CurrentObject = new MarkdownCodeObject(CurrentObject);
        }
        public void PushParagraphWrapper()
        {
            CloseAndPopText();
            CurrentObject = new MarkdownParagraphObject(CurrentObject);
        }
        public void CloseAndPop()
        {
            CurrentObject.Closed = true;
            CurrentObject = CurrentObject.Parent;
        }
        public bool CloseAndPopText()
        {
            if (!(CurrentObject is MarkdownTextObject))
                return false;
            CurrentObject.Closed = true;
            CurrentObject = CurrentObject.Parent;
            return true;
        }
        public void AppendText(string s, bool noHtmlScreening = false)
        {
            if (!noHtmlScreening)
                s = s.ApplyHtmlScreening();
            CurrentTextObject.Append(s);
        }

        public override string ToString() => Root.ToString();
    }
}
using System;

namespace Markdown
{
    class MardownProcessorState
    {
        public bool Finished => Position + 1 >= Source.Length;
        public int Position { get; set; } = 0;
        public string Source { get; }
        public char CurrentChar => Source[Position];
        public char? PreviousChar => Position <= 0 ? null : (char?)Source[Position - 1];
        public char? NextChar => Position + 1 >= Source.Length ? null : (char?) Source[Position + 1];

        public MarkdownRootObject Root { get; } = new MarkdownRootObject();
        public MarkdownObject CurrentObject { get; set; }
        public bool Screening { get; set; } = false;
        public MardownProcessorState(string src)
        {
            Source = src;
            CurrentObject = Root;
        }

        public bool IsSeporator(char? c)
        {
            if (c == null)
                return true;
            if (char.IsWhiteSpace(c.Value))
                return true;
            if (char.IsPunctuation(c.Value))
                return true;
            return false;
        }

        public bool ShouldNotGenerateSubconstructions => CurrentObject.Parent is MarkdownCodeObject;

        public override string ToString() => Root.ToString();

        public bool MakeStep()
        {
            if (Finished)
                return false;

            var shouldNotSkipScreening = false;
            switch (CurrentChar)
            {
                case '\\':
                    ProcessSlash();
                    shouldNotSkipScreening = true;
                    break;
                case '_':
                    ProcessUnderline();
                    break;
                case '`':
                    ProcessGrave();
                    break;
                case '\r':
                    ProcessNewLine();
                    break;
                case '\n':
                    ProcessNewLine();
                    break;
                default:
                    AppendText(CurrentChar+"");
                    break;
            }
            if (!shouldNotSkipScreening)
                Screening = false;
            ++Position;
            return !Finished;
        }
        private void ProcessGrave()
        {
            if (Screening)
            {
                ApplyScreening();
                return;
            }

            if (CurrentObject.Parent is MarkdownCodeObject) // closing `
            {
                CloseAndPop(); // text->code
                CloseAndPop(); // code->???
                return;
            }

            PushCodeWrapper();
            PushText();
        }
        private void ProcessUnderline()
        {
            if (TryApplyScreening()) return;
            if (LookForward(2) == "__")
                ProcessDoubleUnderline();
            else 
                ProcessSingleUnderline();
        }
        private void ProcessSingleUnderline()
        {
            if (CurrentObject.Parent is MarkdownEmObject)
            {
                if (IsSeporator(NextChar)) // closing _
                {
                    CloseAndPop(); // text -> em
                    CloseAndPop(); // em -> ???
                }
                else AppendText("_");
                return;
            }
            if (IsSeporator(PreviousChar)) // opening _
            {
                PushEmWrapper();
                PushText();
                return;
            }
            AppendText("_");
        }
        private void ProcessDoubleUnderline()
        {
            if (IsSeporator(PreviousChar)) // opening __
            {
                PushStrongWrapper();
                PushText();

                ++Position;
                return;
            }
            ++Position;
            if (CurrentObject.Parent is MarkdownStrongObject && IsSeporator(NextChar)) // closing __
            {
                CloseAndPop(); // text -> strong
                CloseAndPop(); // strong -> ???
                return;
            }
            AppendText("__");
        }
        private void ProcessSlash()
        {
            if (Screening)
                ApplyScreening();
            else
                Screening = true;
        }
        private void ProcessNewLine()
        {
            if (ShouldNotGenerateSubconstructions)
            {
                AppendText(CurrentChar + "");
                return;
            }

            var slashR = CurrentChar == '\r';

            if ((LookForward(4) == "\r\n\r\n") || (LookForward(2) == "\n\n"))
            {
                Position += slashR ? 3 : 1;
                if (CurrentObject is MarkdownRootObject)
                    return;

                if (CurrentObject is MarkdownParagraphObject)
                    CloseAndPop();
                else if (!(CurrentObject.Parent is MarkdownParagraphObject))
                {
                    CloseAndPop(); // ??(text) -> ??(not a paragraph)
                    CurrentObject.ReplaceMeWithMyChildren();
                    CloseAndPop(); // ??(not a paragraph) -> ???
                }
                else
                {
                    CloseAndPop(); // text -> p
                    CloseAndPop(); // p -> ???
                }

                PushParagraphWrapper();
                PushText();
                return;
            }

            /*if (LookForward(2) == "\r\n")
            {
                Position += 2;
                AppendText("<br/>", true);
                return;
            }
            if (LookForward(1) == "\n")
            {
                AppendText("<br/>", true);
                return;
            }*/
        }
        private bool TryApplyScreening()
        {
            if (!Screening && !ShouldNotGenerateSubconstructions)
                return false;
            ApplyScreening();
            return true;
        }
        private void ApplyScreening()
        {
            AppendText(CurrentChar + "");
            Screening = false;
        }
        private void CloseAndPop()
        {
            CurrentObject.Closed = true;
            CurrentObject = CurrentObject.Parent;
        }
        private bool CloseAndPopText()
        {
            if (!(CurrentObject is MarkdownTextObject))
                return false;
            CurrentObject.Closed = true;
            CurrentObject = CurrentObject.Parent;
            return true;
        }
        private void PushText()
        {
            CloseAndPopText();
            CurrentObject = new MarkdownTextObject(CurrentObject);
        }
        private void PushStrongWrapper()
        {
            CloseAndPopText();
            CurrentObject = new MarkdownStrongObject(CurrentObject);
        }
        private void PushEmWrapper()
        {
            CloseAndPopText();
            CurrentObject = new MarkdownEmObject(CurrentObject);
        }
        private void PushCodeWrapper()
        {
            CloseAndPopText();
            CurrentObject = new MarkdownCodeObject(CurrentObject);
        }
        private void PushParagraphWrapper()
        {
            CloseAndPopText();
            CurrentObject = new MarkdownParagraphObject(CurrentObject);
        }
        private void AppendTextChar(char c)
        {
            var textObject = CurrentObject as MarkdownTextObject;
            if (textObject != null)
            {
                textObject.AppendChar(c);
                return;
            }

            var rootObject = CurrentObject as MarkdownRootObject;
            if (rootObject != null)
            {
                PushParagraphWrapper();
                PushText();
                (CurrentObject as MarkdownTextObject).AppendChar(c);
                return;
            }
            PushText();
            (CurrentObject as MarkdownTextObject).AppendChar(c);
        }
        private void AppendText(string s, bool noHtmlScreening = false)
        {
            if (!noHtmlScreening)
                s = s.ApplyHtmlScreening();
            foreach (var c in s)
                AppendTextChar(c);
        }
        private string LookForward(int symbols) => Source.Substring(Position, Math.Min(symbols, Source.Length - Position));
        public void FixUnclosedTags()
        {
            while (CurrentObject != null && !CurrentObject.Closed)
            {
                if (CloseAndPopText())
                    continue;
                if (CurrentObject is MarkdownParagraphObject || CurrentObject is MarkdownRootObject)
                {
                    CloseAndPop();
                    continue;
                }
                var temp = CurrentObject;
                CloseAndPop();
                if (temp is MarkdownCodeObject)
                    AppendText("`");
                else if (temp is MarkdownEmObject)
                    AppendText("_");
                else if (temp is MarkdownStrongObject)
                    AppendText("__");
                temp.ReplaceMeWithMyChildren();
            }
        }
    }
}
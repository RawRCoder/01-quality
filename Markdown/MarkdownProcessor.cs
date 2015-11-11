﻿using System;
using System.IO;

namespace Markdown
{
    public class MarkdownProcessor
    {
        private MardownProcessorState State { get; set; }
        public string Process(string markdownText)
        {
            State = new MardownProcessorState(markdownText);
            while (MakeStep()) {}
            FixUnclosedTags();
            return State.ToString();
        }

        public string Process(StreamReader r) 
            => Process(r.ReadToEnd());
        public string ProcessFromFile(string fileName) 
            => Process(File.ReadAllText(fileName));


        private bool MakeStep()
        {
            if (State.Finished)
                return false;

            var shouldNotSkipScreening = false;
            switch (State.CurrentChar)
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
                    State.AppendText(State.CurrentChar + "");
                    break;
            }
            if (!shouldNotSkipScreening)
                State.Screening = false;
            ++State.Position;
            return !State.Finished;
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
        public void FixUnclosedTags()
        {
            while (State.CurrentObject != null && !State.CurrentObject.Closed)
            {
                if (State.CloseAndPopText())
                    continue;
                if (State.CurrentObject is MarkdownParagraphObject || State.CurrentObject is MarkdownRootObject)
                {
                    State.CloseAndPop();
                    continue;
                }
                var temp = State.CurrentObject;
                State.CloseAndPop();
                var text = "";
                if (temp is MarkdownCodeObject)
                    text = "`";
                else if (temp is MarkdownEmObject)
                    text = "_";
                else if (temp is MarkdownStrongObject)
                    text = "__";
                State.AppendText(text);
                temp.ReplaceMeWithMyChildren();
            }
        }
        private void ProcessGrave()
        {
            if (State.Screening)
            {
                ApplyScreening();
                return;
            }

            if (State.CurrentObject is MarkdownCodeObject) // closing `
            {
                State.CloseAndPop();
                return;
            }

            State.PushCodeWrapper(); // opening `
        }
        private void ProcessUnderline()
        {
            if (TryApplyScreening()) return;
            if (State.LookForward(2) == "__")
                ProcessDoubleUnderline();
            else
                ProcessSingleUnderline();
        }
        private void ProcessSingleUnderline()
        {
            if (IsSeporator(State.PreviousChar)) // opening _
            {
                State.PushEmWrapper();
                return;
            }
            if (!(State.CurrentObject is MarkdownParagraphObject) && IsSeporator(State.NextChar)) // closing _
            {
                if (!(State.CurrentObject is MarkdownEmObject))
                {
                    var unclosedTag = State.CurrentObject;
                    State.CloseAndPop();
                    State.AppendText("_");
                    unclosedTag.ReplaceMeWithMyChildren();
                }
                else
                    State.CloseAndPop();
                return;
            }
            State.AppendText("_");
        }
        private void ProcessDoubleUnderline()
        {
            if (IsSeporator(State.PreviousChar)) // opening __
            {
                State.PushStrongWrapper();
                ++State.Position;
                return;
            }
            ++State.Position;
            if (!(State.CurrentObject is MarkdownParagraphObject) && IsSeporator(State.NextChar)) // closing __
            {
                if (!(State.CurrentObject is MarkdownStrongObject))
                {
                    var unclosedTag = State.CurrentObject;
                    State.CloseAndPop();
                    State.AppendText("__");
                    unclosedTag.ReplaceMeWithMyChildren();
                }
                else
                    State.CloseAndPop();
                return;
            }
            State.AppendText("__");
        }
        private void ProcessSlash()
        {
            if (State.Screening)
                ApplyScreening();
            else
                State.Screening = true;
        }
        private void ProcessNewLine()
        {
            if (State.ShouldNotGenerateSubconstructions)
            {
                State.AppendText("" + State.CurrentChar);
                return;
            }

            var windowsStyle = (State.LookForward(4) == "\r\n\r\n");

            if (windowsStyle || (State.LookForward(2) == "\n\n") || (State.LookForward(2) == "\r\r"))
            {
                State.Position += windowsStyle ? 3 : 1;
                if (State.CurrentObject is MarkdownRootObject)
                    return;

                if (State.CurrentObject is MarkdownParagraphObject)
                {
                    State.CloseAndPop();
                }
                else
                {
                    State.CurrentObject.ReplaceMeWithMyChildren();
                    State.CloseAndPop(); 
                }

                State.PushParagraphWrapper();
                return;
            }

            if (State.LookForward(2) == "\r\n")
            {
                ++State.Position;
                State.AppendText(" ");
            }
            else if (State.LookForward(1) == "\r" || State.LookForward(1) == "\n")
                State.AppendText(" ");
        }
        private bool TryApplyScreening()
        {
            if (!State.Screening && !State.ShouldNotGenerateSubconstructions)
                return false;
            ApplyScreening();
            return true;
        }
        private void ApplyScreening()
        {
            State.AppendText(State.CurrentChar + "");
            State.Screening = false;
        }
    }
}

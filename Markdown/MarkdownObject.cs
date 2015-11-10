using System;
using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
    public abstract class MarkdownObject
    {
        public bool Closed { get; set; } = false;
        public MarkdownObject Parent { get; }
        public bool IsRoot => Parent == null;
        public List<MarkdownObject> Subobjects { get; } = new List<MarkdownObject>();
        public string SubobjectsString => string.Join("", Subobjects.Select(o => o.ToString()));

        protected MarkdownObject(MarkdownObject owner = null)
        {
            Parent = owner;
            owner?.Subobjects.Add(this);
        }

        public void ReplaceMeWithMyChildren()
        {
            if (IsRoot)
                throw new Exception("Cannot replace root");

            Parent.Subobjects.Remove(this);
            Parent.Subobjects.AddRange(Subobjects);
        }
    }

    public class MarkdownTextObject : MarkdownObject
    {
        public MarkdownTextObject(MarkdownObject owner = null) : base(owner) { }

        public string Text { get; set; }
        public override string ToString() => Text;
        public bool Empty => string.IsNullOrEmpty(Text);
        public void AppendChar(char c)
        {
            if (Empty)
                Text = "" + c;
            else
                Text += c;
        }
    }

    public abstract class MarkdownTagWrapperObject : MarkdownObject
    {
        protected MarkdownTagWrapperObject(MarkdownObject owner = null) : base(owner) { }
        public abstract string Tag { get; }
        public override string ToString() => SubobjectsString.WrapWithTag(Tag);
    }

    public class MarkdownCodeObject : MarkdownTagWrapperObject
    {
        public MarkdownCodeObject(MarkdownObject owner = null) : base(owner) { }
        public override string Tag => "code";
    }
    public class MarkdownStrongObject : MarkdownTagWrapperObject
    {
        public MarkdownStrongObject(MarkdownObject owner = null) : base(owner) { }
        public override string Tag => "strong";
    }
    public class MarkdownParagraphObject : MarkdownTagWrapperObject
    {
        public MarkdownParagraphObject(MarkdownObject owner = null) : base(owner) { }
        public override string Tag => "p";
    }
    public class MarkdownEmObject : MarkdownTagWrapperObject
    {
        public MarkdownEmObject(MarkdownObject owner = null) : base(owner) { }
        public override string Tag => "em";
    }
    public class MarkdownRootObject : MarkdownObject
    {
        public override string ToString() => ("\r\n" + "<meta charset=\"utf-8\"/>".WrapWithTag("head") + "\r\n" + SubobjectsString.WrapWithTag("body") + "\r\n").WrapWithTag("html");
    }
}
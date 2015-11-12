using System;
using System.IO;
using System.Windows.Forms;

namespace Markdown
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine($"Usage: {Path.GetFileName(Application.ExecutablePath)} <fileName> <[targetFileName]>");
                return;
            }
            var filename = args[0];
            var targetFileName = args.Length >= 2 ? args[1] : filename + ".html";
            var p = new MarkdownProcessor();
            File.WriteAllText(targetFileName, WrapWithHtmlHeadAndBody(p.ProcessFromFile(filename)));
        }

        static string WrapWithHtmlHeadAndBody(string s) =>
            ("\r\n" + "<meta charset=\"utf-8\"/>".WrapWithTag("head") + "\r\n"
             + s.WrapWithTag("body") + "\r\n").WrapWithTag("html");
    }
}

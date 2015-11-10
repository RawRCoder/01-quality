using System.IO;

namespace Markdown
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
                return;
            var filename = args[0];
            var f = File.OpenText(filename);
            var nf = File.CreateText(filename + ".html");
            var p = new MarkdownProcessor();
            nf.Write(p.Process(f));
            f.Close();
            nf.Close();
        }
    }
}

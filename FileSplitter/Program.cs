using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileSplitter
{
    class Program
    {
        static int _verbosity = 0;
        static void Main(string[] args)
        {
            bool show_help = false;
            string fileName = null;
            long size = 52428800; // 50 MB
            bool distinct = false;

            var p = new OptionSet() {
            { "i|input=", "filename to work with",
              n => fileName = Path.GetFullPath(n) },
            { "d|distinct", "only bother with non-duplicated lines",
              (x) => distinct = true},
            { "s|size=", "chunk size for file (ex. 50MB)",
              (string s) => {
                var ctx = new FileSizeContext(s);
                var parser = new FileSizeParser();
                parser.Interpret(ctx);
                size =  ctx.Output; 
              }},
            { "v", "increase debug message verbosity",
              v => { if (v != null) ++_verbosity; } },
            { "h|help",  "show this message and exit", 
              v => show_help = v != null },
        };

            try
            {
                p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("filesplitter: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `filesplitter --help' for more information.");
                return;
            }

            if (show_help || fileName == null)
            {
                ShowHelp(p);
                return;
            }


            //do splitting
            SplitFile(fileName, size, distinct);
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static void Debug(string format, params object[] args)
        {
            if (_verbosity > 0)
            {
                Console.Write("# ");
                Console.WriteLine(format, args);
            }
        }

        public static void SplitFile(string inFile, long upperBound, bool distinctLines)
        {
            Console.WriteLine(inFile);
            using (var reader = new System.IO.StreamReader(inFile))
            {

                var folder = Path.GetDirectoryName(inFile);
                var rootName = Path.GetFileNameWithoutExtension(inFile);
                var ext = Path.GetExtension(inFile);
                var count = 1;
                var filename = string.Format("{0}/{1}.{2}{3}", folder, rootName, count, ext);
                long written = 0;
                string line;

                var writer = new System.IO.StreamWriter(filename);

                var previousLines = new HashSet<string>();
                bool skip;
                while ((line = reader.ReadLine()) != null)
                {
                    skip = false;
                    if (distinctLines)
                    {
                        if (!previousLines.Add(line))
                            skip = true;
                    }

                    if (!skip)
                    {
                        written += line.Length;

                        writer.WriteLine(line);
                        //Debug(buffer.Length);
                        if (written >= upperBound)
                        {
                            writer.Close();
                            Console.WriteLine(string.Format("Wrote file: {0}", filename, written));
                            written = 0;
                            ++count;

                            filename = string.Format("{0}/{1}.{2}{3}", folder, rootName, count, ext);
                            writer = new System.IO.StreamWriter(filename);
                        }
                    }

                }
                writer.Flush();
                writer.Close();
                Console.WriteLine(string.Format("Wrote file: {0}", filename, written));
                reader.Close();
            }
        }
    }
}


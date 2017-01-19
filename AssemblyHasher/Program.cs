using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AssemblyHasher
{
    class Program
    {
        static void Main(string[] args)
        {
            bool ignoreVersions = false;
            string outPath = null;

            var arguments = args.ToList();
            if (arguments.Any(arg => arg == "--ignore-versions"))
            {
                arguments.RemoveAll(arg => arg == "--ignore-versions");
                ignoreVersions = true;
            }

            if (arguments.Any(a => a.Contains("--output-path:")))
            {
                var arg = arguments.Where(a => a.Contains("--output-path:")).FirstOrDefault();
                if (arg != null)
                {
                    outPath = arg.Split(':')[1];
                    arguments.Remove(arg);
                }

            }

            if (args.Length < 1)
            {
                Console.WriteLine("Specify assembly filenames to hash");
                Console.WriteLine("   --ignore-versions: ignore assembly version and assembly file version attributes");
                return;


            }

            var m = new Manifest();

            var hash = FileHasher.Hash(ignoreVersions, out m, arguments.ToList());
            Console.Write(hash);

            if(!string.IsNullOrEmpty(outPath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(Manifest));
                using (MemoryStream ms = new MemoryStream())
                {
                    xs.Serialize(ms, m);
                    ms.Position = 0;
                    using (StreamReader sr = new StreamReader(ms))
                    {
                        var result = sr.ReadToEnd();
                        File.WriteAllText(outPath, result);
                    }

                }
            }

            Console.Read();
        }
    }
}

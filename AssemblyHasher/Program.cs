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
            bool keepDisassembly = false;
            bool noExit = false;
            string tempPath = null;

            var arguments = args.ToList();

            ignoreVersions = arguments.ResolveArgument<bool>("--ignore-versions",false, true);
            outPath = arguments.ResolveArgument<string>("--output-path", null);
            keepDisassembly = arguments.ResolveArgument<bool>("--keepFiles", false,true);
            tempPath = arguments.ResolveArgument<string>("--temp-path", null);
            noExit = arguments.ResolveArgument<bool>("--noExit", false, true);


            if (tempPath != null)
                Disassembler.TempPathToUse = tempPath;

            if (args.Length < 1)
            {
                Console.WriteLine("Specify assembly filenames to hash");
                Console.WriteLine("   --ignore-versions: ignore assembly version and assembly file version attributes");
                Console.WriteLine("   --output-path: path to place a generated manfiest of the hash and children hashes");
                Console.WriteLine("   --keepFiles: don't delete the IL and RES files that are created during disassembly");
                Console.WriteLine("   --temp-path: what path to use for extracting temporary files");
                Console.WriteLine("   --noExit: ends program with a ReadLine call so it doesn't exit");
                return;
            }

            var m = new Manifest();

            var hash = FileHasher.Hash(ignoreVersions, out m, arguments.ToList(), keepDisassembly);
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

            if(noExit)
                Console.Read();
        }
    }

    
}

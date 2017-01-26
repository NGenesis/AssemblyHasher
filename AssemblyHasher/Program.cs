using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            bool showTrace = false;

            var arguments = args.ToList();

            ignoreVersions = arguments.ResolveArgument<bool>("--ignore-versions",false, true);
            outPath = arguments.ResolveArgument<string>("--output-path", null);
            keepDisassembly = arguments.ResolveArgument<bool>("--keepFiles", false,true);
            tempPath = arguments.ResolveArgument<string>("--temp-path", null);
            noExit = arguments.ResolveArgument<bool>("--noExit", false, true);
            showTrace = arguments.ResolveArgument<bool>("--verbose", false, true);


            if (showTrace)
                Trace.Listeners.Add(new ConsoleTraceListener());

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
                Console.WriteLine("   --verbose: turns on console logging to show program progress");
                return;
            }

            

            HashProcess hp = new HashProcess();
            hp.ignoreVersions = ignoreVersions;
            hp.keepDisassembly = keepDisassembly;
            hp.outPath = outPath;
            hp.tempPath = tempPath;

            //start the process
            Trace.WriteLine("Starting the HashProcess");
            hp.Start(arguments);
            Console.WriteLine(hp.Current.MasterHash);
            Trace.WriteLine("HashProcess has completed");



            if (noExit)
                Console.Read();

            //cleanup
            try
            {
                Trace.WriteLine("Removing ILDASM");
                File.Delete(Disassembler.ILDasmFileLocation);
            }
            catch { }

            Trace.WriteLine("Exiting...");
        }
    }

    
}

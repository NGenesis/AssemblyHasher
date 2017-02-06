using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AssemblyHasher
{

    public class Disassembler
    {
        public class DissasembleOutput
        {
            public string Folder { get; private set; }
            public string ILFilename { get; private set; }
            public bool Successful { get; set; }
            public string[] Resources { get; private set; }
            public void Delete()
            {
                if(Successful) //if we are successful, that means we have IL and RES files to delete in this folder
                    Directory.Delete(Folder, true);
            }

            public DissasembleOutput(string folder, string ilFilename, bool success=true)
            {
                Successful = success;
                Folder = folder;
                ILFilename = ilFilename;
                if (success)
                {
                    //get all the resources to
                    Resources = Directory.EnumerateFiles(Folder)
                        .Where(filename => filename != ilFilename && !regexPostSharpResourceFiles.IsMatch(Path.GetFileName(filename) ?? ""))
                        .ToArray();
                }
            }
        }

        public static Regex regexPostSharpResourceFiles = new Regex("^PostSharp\\.Aspects\\.[0-9\\.]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Lazy<Assembly> currentAssembly = new Lazy<Assembly>(() =>
        {
            return MethodBase.GetCurrentMethod().DeclaringType.Assembly;
        });

        private static readonly Lazy<string> executingAssemblyPath = new Lazy<string>(() =>
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        });

        private static readonly Lazy<string[]> arrResources = new Lazy<string[]>(() =>
        {
            return currentAssembly.Value.GetManifestResourceNames();
        });

        private const string ildasmArguments = "/all /text \"{0}\" /output:\"{1}\"";

        public static string ILDasmFileLocation
        {
            get
            {
                return Path.Combine(executingAssemblyPath.Value, "ildasm.exe");
            }
        }

        static Disassembler()
        {
            if (!File.Exists(ILDasmFileLocation))
            {
                //extract the ildasm file to the executing assembly location
                ExtractFileToLocation("ildasm.exe", ILDasmFileLocation);
            }
        }

        /// <summary>
        /// Saves the file from embedded resource to a given location.
        /// </summary>
        /// <param name="embeddedResourceName">Name of the embedded resource.</param>
        /// <param name="fileName">Name of the file.</param>
        protected static void SaveFileFromEmbeddedResource(string embeddedResourceName, string fileName)
        {
            if (File.Exists(fileName))
            {
                //the file already exists, we can add deletion here if we want to change the version of the 7zip
                return;
            }
            FileInfo fileInfoOutputFile = new FileInfo(fileName);

            using (FileStream streamToOutputFile = fileInfoOutputFile.OpenWrite())
            using (Stream streamToResourceFile = currentAssembly.Value.GetManifestResourceStream(embeddedResourceName))
            {
                const int size = 4096;
                byte[] bytes = new byte[4096];
                int numBytes;
                while ((numBytes = streamToResourceFile.Read(bytes, 0, size)) > 0)
                {
                    streamToOutputFile.Write(bytes, 0, numBytes);
                }

                streamToOutputFile.Close();
                streamToResourceFile.Close();
            }
        }

        /// <summary>
        /// Searches the embedded resource and extracts it to the given location.
        /// </summary>
        /// <param name="fileNameInDll">The file name in DLL.</param>
        /// <param name="outFileName">Name of the out file.</param>
        protected static void ExtractFileToLocation(string fileNameInDll, string outFileName)
        {
            string resourcePath = arrResources.Value.FirstOrDefault(resource => resource.EndsWith(fileNameInDll, StringComparison.InvariantCultureIgnoreCase));
            if (resourcePath == null)
            {
                throw new Exception(string.Format("Cannot find {0} in the embedded resources of {1}", fileNameInDll, currentAssembly.Value.FullName));
            }
            SaveFileFromEmbeddedResource(resourcePath, outFileName);
        }

        public static string TempPathToUse { get; set; }
        private static string GetTemporalFolder(string hint=null)
        {
            var path = Path.Combine((TempPathToUse != null ? TempPathToUse : Path.GetTempPath()), hint != null ? Path.GetFileNameWithoutExtension(hint) : Path.GetRandomFileName());
            while (Directory.Exists(path) || File.Exists(path))
            {
                path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }
            Directory.CreateDirectory(path);
            return path;
        }

        public static DissasembleOutput Disassemble(string assemblyFilename)
        {
            if (!File.Exists(assemblyFilename))
            {
                throw new FileNotFoundException(string.Format("The file {0} does not exist!", assemblyFilename));
            }

            var outputFolder = GetTemporalFolder(assemblyFilename);

            var startInfo = new ProcessStartInfo(ILDasmFileLocation, string.Format(ildasmArguments,
               Path.GetFullPath(assemblyFilename), "output.il"));
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = outputFolder;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            using (var process = new Process { StartInfo = startInfo })
            {
                string output = "";
                process.OutputDataReceived += (sender, args) =>
                    {
                        output += args.Data + Environment.NewLine;
                    };
                process.ErrorDataReceived += (sender, args) =>
                {
                    output += args.Data + Environment.NewLine;
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode > 0)
                {
                    //this wasn't a .NET compatible assembly, just hash its contents
                    return new DissasembleOutput(Path.GetDirectoryName(assemblyFilename), assemblyFilename, success: false);

                    throw new InvalidOperationException(
                        string.Format("Generating IL code for file {0} failed with exit code - {1}. Log: {2}",
                        assemblyFilename, process.ExitCode, output));
                }
            }

            var ilFilename = Path.Combine(outputFolder, "output.il");
            return new DissasembleOutput(outputFolder, ilFilename);
        }
    }
}

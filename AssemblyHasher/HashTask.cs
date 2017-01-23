using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyHasher
{
    public class HashTask : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string OutputDirectory { get; set; }

        public string ManifestLocation { get; set; }

        public bool IgnoreVersions { get; set; }

        public override bool Execute()
        {
            //send this to our hash routines
            HashProcess hp = new HashProcess();
            hp.ignoreVersions = this.IgnoreVersions;

            var arguments = new List<string> { OutputDirectory };
            //start the process
            hp.Start(arguments);

            //save hte manifest
            var xml = hp.GetManifestXML();

            File.WriteAllText(Path.Combine(OutputDirectory, "HashManifest.xml"), xml);

            return true;

        }
    }
}

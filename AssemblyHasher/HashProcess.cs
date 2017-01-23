using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AssemblyHasher
{
    public class HashProcess
    {
        public Manifest Current { get; set; }
        public bool ignoreVersions = false;
        public string outPath = null;
        public bool keepDisassembly = false;
        public bool noExit = false;
        public string tempPath = null;

        public void Start(List<string> arguments)
        {
            var m = new Manifest();
            var hash = FileHasher.Hash(ignoreVersions, out m, arguments.ToList(), keepDisassembly);
            Console.Write(hash);

            this.Current = m;

            if (!string.IsNullOrEmpty(outPath))
            {
                SaveManifest(outPath, GetManifestXML());
            }
        }

        public string GetManifestXML()
        {
            XmlSerializer xs = new XmlSerializer(typeof(Manifest));
            using (MemoryStream ms = new MemoryStream())
            {
                xs.Serialize(ms, Current);
                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms))
                {
                    var result = sr.ReadToEnd();
                    return result;
                }
            }
        }

        public void SaveManifest(string outPath, string result)
        {
            File.WriteAllText(outPath, result);
        }

        
    }
}

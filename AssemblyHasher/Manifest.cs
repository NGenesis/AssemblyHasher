using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AssemblyHasher
{
    public class Manifest
    {
        public Manifest() { this.Components = new List<ChildItem>(); }

        [XmlAttribute]
        public string MasterHash { get; set; }

        public List<ChildItem> Components { get; set; }
    }

    public class ChildItem
    {
        [XmlAttribute]
        public string Path { get; set; }

        [XmlAttribute]
        public string Hash { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace MicroControllerOptimizer.XMLSerialization
{
    public class config
    {
        [XmlAttribute]
        public string name;
        [XmlElement]
        public general[] general;
        [XmlElement]
        public execution execution;
        [XmlElement]
        public memory memory;
	}
}

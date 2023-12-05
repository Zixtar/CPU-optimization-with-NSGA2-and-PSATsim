using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MicroControllerOptimizer.XMLSerialization
{
    public class execution
    {
        [XmlAttribute]
        public string architecture;
		[XmlAttribute]
		public int integer;
		[XmlAttribute]
		public int floating;
		[XmlAttribute]
		public int branch;
		[XmlAttribute]
		public int memory;
	}
}

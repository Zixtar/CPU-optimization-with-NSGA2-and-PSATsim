using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Converters;
using System.Xml.Serialization;
namespace MicroControllerOptimizer.XMLSerialization
{
    public class general
    {
        [XmlAttribute]
        public int superscalar;
        [XmlAttribute]
        public int rename;
        [XmlAttribute]
        public int reorder;
		[XmlAttribute]
		public string rsb_architecture;
		[XmlAttribute]
		public int rs_per_rsb;
		[XmlAttribute]
		public bool speculative;
		[XmlAttribute]
		public double speculation_accuracy;
		[XmlAttribute]
		public bool separate_dispatch;
		[XmlAttribute]
		public int seed;
		[XmlAttribute]
		public string trace;
		[XmlAttribute]
		public string output;
		[XmlAttribute]
		public double vdd;
		[XmlAttribute]
		public double frequency;

	}
}

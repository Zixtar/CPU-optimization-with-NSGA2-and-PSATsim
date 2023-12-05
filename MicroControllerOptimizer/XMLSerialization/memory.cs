using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MicroControllerOptimizer.XMLSerialization
{
	public class memory
	{
		[XmlAttribute]
		public memArchitecture architecture;
		[XmlElement]
		public memoryParameters system;
		[XmlElement]
		public memoryParameters? l1_code;
		[XmlElement]
		public memoryParameters? l1_data;
		[XmlElement]
		public memoryParameters? l2;
	}

	public struct memoryParameters
	{
		[XmlAttribute]
		public double hitrate;
		[XmlAttribute]
		public int latency;
	}

	public enum memArchitecture
	{
		system,
		l1,
		l2,
	}
}

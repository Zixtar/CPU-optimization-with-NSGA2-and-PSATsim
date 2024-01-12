using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MicroControllerOptimizer.XMLSerialization
{
    public class memoryClass
    {
        static public memoryClass SetMemoryValues()
        {
            var memory = new memoryClass();
            memory.architecture = memArchitecture.l2;

            memory.system = new memoryParameters();
            memory.l1_code = new memoryParameters();
            memory.l1_data = new memoryParameters();
            memory.l2 = new memoryParameters();
            memory.system.latency = 20;
            memory.system.hitrate = 1;
            memory.l1_code.hitrate = 0.990;
            memory.l1_code.latency = 1;
            memory.l1_data.hitrate = 0.980;
            memory.l1_data.latency = 1;
            memory.l2.hitrate = 0.950;
            memory.l2.latency = 8;


            return memory;
        }
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

    public class memoryParameters
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

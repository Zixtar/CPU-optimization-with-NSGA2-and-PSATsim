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
        static public memoryClass GenerateRandomMemory()
        {
            var memory = new memoryClass();
            memory.architecture = memArchitecture.l2;
            do
            {
                memory.system = GenerateRandomMemoryParams();
                memory.system.hitrate = 1;
                memory.l1_code = GenerateRandomMemoryParams();
                memory.l1_data = GenerateRandomMemoryParams();
                memory.l2 = GenerateRandomMemoryParams();
            } while (memory.BadConfiguration());

            return memory;
        }
        public bool BadConfiguration()
        {
            if (l1_code.latency > l2.latency || l1_data.latency > l2.latency || l2.latency > system.latency)
                return true;

            return false;
        }
        [XmlAttribute]
        public memArchitecture architecture;

        [XmlElement]
        public memoryParameters system;
        static memoryParameters GenerateRandomMemoryParams()
        {
            Random mem = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            var memory = new memoryParameters();
            memory.hitrate = mem.NextDouble();
            memory.latency = mem.Next(1, 100);

            return memory;
        }
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

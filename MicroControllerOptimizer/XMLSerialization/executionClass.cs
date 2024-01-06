using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MicroControllerOptimizer.XMLSerialization
{
    public class executionClass
    {
        public static executionClass GenerateRandomExecution()
        {
            var execution = new executionClass();
            
            execution.CreateRandomInteger();
            execution.CreateRandomFloating();
            execution.CreateRandomBranch();
            execution.CreateRandomMemory();

            return execution;
        }

        [XmlAttribute]
        public string architecture = "standard";
        [XmlAttribute]
		public int integer;
        void CreateRandomInteger()
        {
            Random integer = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            this.integer = integer.Next(1, 9);
        }
        [XmlAttribute]
		public int floating;
        void CreateRandomFloating()
        {
            Random floating = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            this.floating = floating.Next(1, 9);
        }
        [XmlAttribute]
		public int branch;
        void CreateRandomBranch()
        {
            Random branch = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            this.branch = branch.Next(1, 9);
        }
        [XmlAttribute]
		public int memory;
        void CreateRandomMemory()
        {
            Random memory = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            this.memory = memory.Next(1, 9);
        }
    }
}

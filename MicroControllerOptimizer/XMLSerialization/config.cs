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
        
        public static config GetRandomConfig()
        {
            var config = new config();
            config.name = "tempName";
            config.general = new generalClass[1];
            config.general[0] = generalClass.GenerateRandomGeneral();
            config.execution = executionClass.GenerateRandomExecution();
            config.memory = memoryClass.GenerateRandomMemory();
            return config;
        }

        [XmlAttribute]
        public string name;
        [XmlElement]
        public generalClass[] general;
        [XmlElement]
        public executionClass execution;
        [XmlElement]
        public memoryClass memory;
	}
}

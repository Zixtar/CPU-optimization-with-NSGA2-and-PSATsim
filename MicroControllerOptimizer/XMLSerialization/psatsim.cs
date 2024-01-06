using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MicroControllerOptimizer.XMLSerialization
{
    public class psatsim
    {
        public static psatsim GetRandomPsatsim()
        {
            var psatsim = new psatsim();
            psatsim.config = config.GetRandomConfig();
            return psatsim;
        }
        [XmlElement]
        public config config;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Converters;
using System.Xml.Serialization;
namespace MicroControllerOptimizer.XMLSerialization
{
    public class generalClass
    {
        static public generalClass GenerateRandomGeneral()
        {
            var general = new generalClass();
            general.CreateRandomSuperscalar();
            general.CreateRandomRename();
            general.CreateRandomReorder();
            general.CreateRandomRsbArch();
            general.CreateRandomVdd();
            general.CreateRandomRsPersRsb();
            general.CreateRandomFrequency();
            general.CreateRandomSeparateDispatch();

            return general;
        }

        public generalClass DeepCopy()
        {
            var copiedGeneral = this.MemberwiseClone() as generalClass;
            return copiedGeneral;
        }
        [XmlAttribute]
        public int superscalar { get; set; }
        void CreateRandomSuperscalar()
        {
            Random superscalar = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            this.superscalar = superscalar.Next(1, 17);
        }
        [XmlAttribute]
        public int rename;
        void CreateRandomRename()
        {
            Random rename = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            this.rename = rename.Next(1, 513);
        }
        [XmlAttribute]
        public int reorder;
        void CreateRandomReorder()
        {
            Random reorder = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            this.reorder = reorder.Next(1, 513);
        }
        [XmlAttribute]
        public string rsb_architecture;
        void CreateRandomRsbArch()
        {
            Random rsb_architecture = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            this.rsb_architecture = ((RsbArch)rsb_architecture.Next(0, 3)).ToString();
        }
        enum RsbArch
        {
            centralized,
            hybrid,
            distributed,
        }
        [XmlAttribute]
        public int rs_per_rsb;
        void CreateRandomRsPersRsb()
        {
            Random rs_per_rsb = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            this.rs_per_rsb = rs_per_rsb.Next(1, 8);
        }
        [XmlAttribute]
        public bool speculative = false;

        [XmlAttribute]
        public double speculation_accuracy = 1;

        [XmlAttribute]
        public bool separate_dispatch;
        void CreateRandomSeparateDispatch()
        {
            Random separate_dispatch = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            this.separate_dispatch = Convert.ToBoolean(separate_dispatch.Next(0, 2));
        }
        [XmlAttribute]
        public int seed = 0;
        [XmlAttribute]
        public string trace;
        [XmlAttribute]
        public string output = "output.xml";
        [XmlAttribute]
        public double vdd;
        void CreateRandomVdd()
        {
            Random vdd = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            this.vdd = vdd.NextDouble()*10;
        }
        [XmlAttribute]
        public double frequency;
        void CreateRandomFrequency()
        {
            Random frequency = new Random(DateTime.Now.TimeOfDay.Nanoseconds);
            this.frequency = frequency.Next(100, 2000);
        }

    }
}

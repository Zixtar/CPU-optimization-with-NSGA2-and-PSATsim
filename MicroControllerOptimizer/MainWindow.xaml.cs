using MicroControllerOptimizer.XMLSerialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;

namespace MicroControllerOptimizer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			var psatsim = new psatsim();
			var configs = new List<config>();
			var config = new config()
			{
				name = "Case3",
			};
			var generals = new List<general>();
			var general = new general()
			{
				superscalar = 1,
				rename = 16,
				reorder = 20,
				rsb_architecture = "distributed",
				rs_per_rsb = 1,
				speculative = true,
				speculation_accuracy = 0.98,
				separate_dispatch = true,
				seed = 1,
				trace = "compress.tra",
				output = "output.xml",
				vdd = 2.2,
				frequency = 600,
			};
			var general2 = new general()
			{
				superscalar = 1,
				rename = 16,
				reorder = 20,
				rsb_architecture = "distributed",
				rs_per_rsb = 1,
				speculative = true,
				speculation_accuracy = 0.98,
				separate_dispatch = true,
				seed = 1,
				trace = "compress.tra",
				output = "output2.xml",
				vdd = 2.2,
				frequency = 700,
			};
			generals.Add(general);
			generals.Add(general2);
			var execution = new execution
			{
				architecture = "standard",
				integer = 2,
				floating = 2,
				branch = 1,
				memory = 1
			};
			var memory = new memory
			{
				architecture = memArchitecture.l2,
				l1_code = new memoryParameters { hitrate=0.99,latency=1},
				l1_data = new memoryParameters { hitrate=0.97,latency=1},
				l2 = new memoryParameters { hitrate=0.99, latency =3},
				system = new memoryParameters { latency=20 },
			};
			config.general = generals.ToArray();
			config.execution = execution;
			config.memory = memory;
			config.name = "Case1";
			configs.Add(config);
			psatsim.config = configs.ToArray();
			var generalXml = new XmlSerializer(typeof(psatsim));
			var xmlNamespace = new XmlSerializerNamespaces();
			xmlNamespace.Add("", "");
			var xmlOptions = new XmlWriterSettings()
			{
				Indent = true,
				OmitXmlDeclaration = true,
			};
			using (var stream = XmlWriter.Create("input.xml", xmlOptions))
			{
				generalXml.Serialize(stream, psatsim, xmlNamespace);
			}

		}
	}
}

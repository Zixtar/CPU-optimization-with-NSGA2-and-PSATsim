﻿using MicroControllerOptimizer.XMLSerialization;
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
using Path = System.IO.Path;

namespace MicroControllerOptimizer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		const string PathToPsatSim = "..\\..\\..\\..\\External\\PSATSim";
		public MainWindow()
		{
			var path = PathToPsatSim;
			InitializeComponent();
			psatsim psatsim = GetDefaultConfiguration();
			//TODO Change parameters in the config
			AddAllTracesToConfiguration(ref psatsim, path);
			WriteconfigurationXML(psatsim, path);
			System.Diagnostics.Process.Start(Path.Combine(path, "psatsim_con.exe"), "input.xml output.xml -Ag");
			GetAvgParameters(path, out double avgCpi, out double avgEnergy);
		}

		private void AddAllTracesToConfiguration(ref psatsim psatsim, string path)
		{
			var traces = Directory.GetFiles(Path.Combine(path, "Traces")).Select(x => Path.GetFileName(x));
			var tempGenerals = new List<general>();
			var firstGeneral = psatsim.config.general.First();
			foreach (var trace in traces)
			{
				firstGeneral.trace = trace;
				tempGenerals.Add(firstGeneral);
			}
			psatsim.config.general = tempGenerals.ToArray();
		}

		private static void GetAvgParameters(string path, out double avgCpi, out double avgEnergy)
		{
			var outputDoc = new XmlDocument();
			if (File.Exists(Path.Combine(path, "output.xml")))
				outputDoc.Load(Path.Combine(path, "output.xml"));

			var variantResults = outputDoc.DocumentElement.SelectNodes("variation/general");
			avgCpi = 0;
			avgEnergy = 0;
			foreach (XmlNode result in variantResults)
			{
				Double.TryParse(result.Attributes["energy"].Value, out double energy);
				avgEnergy += energy;
				Double.TryParse(result.Attributes["ipc"].Value, out double ipc);
				avgCpi += 1/ipc;
			}
			avgEnergy /= variantResults.Count;
			avgCpi /= variantResults.Count;
		}

		private static void WriteconfigurationXML(psatsim psatsim, string path)
		{
			var generalXml = new XmlSerializer(typeof(psatsim));
			var xmlNamespace = new XmlSerializerNamespaces();
			xmlNamespace.Add("", "");
			var xmlOptions = new XmlWriterSettings()
			{
				Indent = true,
				OmitXmlDeclaration = true,
			};
			using (var stream = XmlWriter.Create(Path.Combine(path, "input.xml"), xmlOptions))
			{
				generalXml.Serialize(stream, psatsim, xmlNamespace);
			}
		}

		private static psatsim GetDefaultConfiguration()
		{
			var psatsim = new psatsim();
			var config = new config()
			{
				name = "Case1",
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
			generals.Add(general);
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
				l1_code = new memoryParameters { hitrate = 0.99, latency = 1 },
				l1_data = new memoryParameters { hitrate = 0.97, latency = 1 },
				l2 = new memoryParameters { hitrate = 0.99, latency = 3 },
				system = new memoryParameters { latency = 20 },
			};
			config.general = generals.ToArray();
			config.execution = execution;
			config.memory = memory;
			config.name = "Case1";
			psatsim.config = config;
			return psatsim;
		}
	}
}

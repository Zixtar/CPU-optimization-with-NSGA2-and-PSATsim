using MicroControllerOptimizer.XMLSerialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
            InitializeComponent();
        }

        private void LaunchPsatsim()
        {
            var path = Path.GetFullPath(PathToPsatSim);

            Int32.TryParse(txtPopSize.Text, out var popSize);
            List<psatsim> configs = new List<psatsim>();
            for (int i = 0; i < popSize; i++)
            {
                configs.Add(psatsim.GetRandomPsatsim());
            }

            RunConfig(configs[0], path);

            GetAvgParameters(path, out double avgCpi, out double avgEnergy);
        }

        private void RunConfig(psatsim psatsimVar, string path)
        {
            AddAllTracesToConfiguration(ref psatsimVar, path);
            WriteconfigurationXML(psatsimVar, path);
            Process? process = RunPsatSimWithInputFile(path);
            while (!process.HasExited) ;
        }

        private static Process? RunPsatSimWithInputFile(string path)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "C:\\windows\\system32\\cmd.exe";
            startInfo.Arguments = $"/K cd /D {path}\\ & .\\psatsim_con.exe input.xml output.xml -Ag";

            var outputFilePath = Path.Combine(path, "output.xml");
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            var process = Process.Start(startInfo);
            return process;
        }

        private void AddAllTracesToConfiguration(ref psatsim psatsim, string path)
        {
            var traces = Directory.GetFiles(Path.Combine(path, "Traces")).Select(x => Path.GetFileName(x));
            var tempGenerals = new List<generalClass>();
            var tempGeneral = psatsim.config.general.First();
            var TracesFolderRelativetoPsatsim = "./Traces";
            foreach (var trace in traces)
            {
                var newGeneral = tempGeneral.DeepCopy();
                newGeneral.trace = $"{TracesFolderRelativetoPsatsim}/{trace}";
                tempGenerals.Add(newGeneral);
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
                avgCpi += 1 / ipc;
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
            var generals = new List<generalClass>();
            var general = new generalClass()
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
            var execution = new executionClass
            {
                architecture = "standard",
                integer = 2,
                floating = 2,
                branch = 1,
                memory = 1
            };
            var memory = new memoryClass
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

        private void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            LaunchPsatsim();
        }

        private psatsim MutatePsatsim(psatsim psatsimVar)
        {
            var rand = new Random();
            if (rand.NextDouble() < Convert.ToDouble(txtMutationProb.Text))
            {
                psatsimVar.MutateRandomVariable(Convert.ToDouble(txtMutationDistance.Text));
            }
            return psatsimVar;
        }

        private void CrossoverPsatsims(ref psatsim psatsimVar1,ref psatsim psatsimVar2)
        {
            var rand = new Random();
            if (rand.NextDouble() < Convert.ToDouble(txtCrossoverProb.Text))
            {
                psatsimVar1.CrossoverRandomVariables(psatsimVar2, Convert.ToDouble(txtCrossoverDistance.Text));
            }
        }

    }
}

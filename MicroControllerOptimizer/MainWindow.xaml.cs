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
        int populationSize;
        NsgaII algorithmInstance = new NsgaII();
        private void LaunchNSGA2()
        {
            algorithmInstance.crossoverProbability = Convert.ToDouble(txtCrossoverProb.Text);
            algorithmInstance.mutationProbability = Convert.ToDouble(txtMutationProb.Text);
            algorithmInstance.mutationDistance = Convert.ToDouble(txtMutationDistance.Text);
            algorithmInstance.crossoverDistance = Convert.ToDouble(txtCrossoverDistance.Text);
            algorithmInstance.currGeneration = 0;
            populationSize = Convert.ToInt32(txtPopSize.Text);
            var maxGenerations = Convert.ToInt32(txtGenerationsNr.Text);

            var path = Path.GetFullPath(PathToPsatSim);

            List<psatsim> configs = new List<psatsim>();
            for (int i = 0; i < populationSize; i++)
            {
                configs.Add(psatsim.GetRandomPsatsim());
                configs[i].config.name = $"Generation: {algorithmInstance.currGeneration}, Individual {i + 1}";
            }

            List<Individual> population = new();

            foreach (var config in configs)
            {
                var count = RunParalelConfigs(config, path);
                GetAvgParameters(count, path, out double avgCpi, out double avgEnergy);
                population.Add(new Individual(config, avgCpi, avgEnergy));
            }

            do
            {
                for (int i = 0; i < population.Count; i++)
                {
                    if (!population[i].UpToDate)
                    {
                        var count = RunParalelConfigs(population[i].Config, path);
                        GetAvgParameters(count, path, out double avgCpi, out double avgEnergy);
                        population[i].Objectives[0] = avgCpi;
                        population[i].Objectives[1] = avgEnergy;
                        population[i].UpToDate = true;
                    }
                }

                var sortedPop = algorithmInstance.FastNonDominatedSort(population);
                foreach (var front in sortedPop)
                {
                    algorithmInstance.CalculateCrowdingDistance(front);
                }

                algorithmInstance.currGeneration++;
                population = algorithmInstance.GenerateNextGeneration(population);
            } while (algorithmInstance.currGeneration < maxGenerations);
        }


        #region serial running
        private void RunConfig(psatsim psatsimVar, string path)
        {
            AddAllTracesToConfiguration(ref psatsimVar, path);
            WriteConfigurationXML(psatsimVar, path);
            Process? process = RunPsatSimWithInputFile(path);
            while (!process.HasExited) ;
        }

        private void AddAllTracesToConfiguration(ref psatsim psatsim, string path)
        {
            var traces = Directory.GetFiles(Path.Combine(path, "Traces")).Select(x => Path.GetFileName(x));
            var tempGenerals = new List<XMLSerialization.generalClass>();
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

        private static void WriteConfigurationXML(psatsim psatsim, string path)
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

        private static Process? RunPsatSimWithInputFile(string path)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "C:\\windows\\system32\\cmd.exe";
            startInfo.Arguments = $"/c cd /D {path}\\ & .\\psatsim_con.exe input.xml output.xml -Ag";

            var outputFilePath = Path.Combine(path, "output.xml");
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            var process = Process.Start(startInfo);
            return process;
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

        #endregion

        #region paralel running
        private int RunParalelConfigs(psatsim psatsimVar, string path)
        {
            var psatsims = GetConfigsForAllTraces(psatsimVar, path);
            WriteAllConfigurationsXML(psatsims, path);
            var processes = RunParalelPsatsism(psatsims, path);
            while (processes.Any(x => !x.HasExited)) ;
            return psatsims.Count;
        }

        private List<psatsim> GetConfigsForAllTraces(psatsim psatsim, string path)
        {
            var traces = Directory.GetFiles(Path.Combine(path, "Traces")).Select(x => Path.GetFileName(x));
            var newPsatsims = new List<psatsim>();
            var tempGeneral = psatsim.config.general.First();
            var TracesFolderRelativetoPsatsim = "./Traces";
            foreach (var trace in traces)
            {
                var newGeneral = tempGeneral.DeepCopy();
                newGeneral.trace = $"{TracesFolderRelativetoPsatsim}/{trace}";
                var newPsatsim = new psatsim();
                newPsatsim.config = psatsim.config;
                newPsatsim.config.general[0] = newGeneral;
                newPsatsims.Add(newPsatsim);
            }
            return newPsatsims;
        }

        private static void WriteAllConfigurationsXML(List<psatsim> psatsims, string path)
        {
            for (int i = 0; i < psatsims.Count; i++)
            {
                var generalXml = new XmlSerializer(typeof(psatsim));
                var xmlNamespace = new XmlSerializerNamespaces();
                xmlNamespace.Add("", "");
                var xmlOptions = new XmlWriterSettings()
                {
                    Indent = true,
                    OmitXmlDeclaration = true,
                };
                using (var stream = XmlWriter.Create(Path.Combine(path, $"input{i}.xml"), xmlOptions))
                {
                    generalXml.Serialize(stream, psatsims[i], xmlNamespace);
                }
            }
        }

        private static List<Process?> RunParalelPsatsism(List<psatsim> psatsims, string path)
        {
            List<Process?> procesess = new();
            for (int i = 0; i < psatsims.Count; i++)
            {
                var startInfo = new ProcessStartInfo();
                startInfo.FileName = "C:\\windows\\system32\\cmd.exe";
                startInfo.Arguments = $"/c cd /D {path}\\ & .\\psatsim_con.exe input{i}.xml output{i}.xml -Ag";

                var outputFilePath = Path.Combine(path, "output.xml");
                if (File.Exists(outputFilePath))
                {
                    File.Delete(outputFilePath);
                }

                var process = Process.Start(startInfo);
                procesess.Add(process);
            }
            return procesess;
        }

        private static void GetAvgParameters(int count, string path, out double avgCpi, out double avgEnergy)
        {
            avgCpi = 0;
            avgEnergy = 0;
            var totalVariants = 0;
            for (int i = 0; i < count; i++)
            {
                var outputDoc = new XmlDocument();
                if (File.Exists(Path.Combine(path, $"output{i}.xml")))
                    outputDoc.Load(Path.Combine(path, $"output{i}.xml"));

                var variantResults = outputDoc.DocumentElement.SelectNodes("variation/general");
                totalVariants += variantResults.Count;
                foreach (XmlNode result in variantResults)
                {
                    Double.TryParse(result.Attributes["energy"].Value, out double energy);
                    avgEnergy += energy;
                    Double.TryParse(result.Attributes["ipc"].Value, out double ipc);
                    avgCpi += 1 / ipc;
                }
            }
            avgEnergy /= totalVariants;
            avgCpi /= totalVariants;
        }

        #endregion


        private void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            LaunchNSGA2();
        }
    }
}

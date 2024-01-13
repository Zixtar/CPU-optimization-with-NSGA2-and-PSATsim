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
        double crossoverProbability;
        double mutationProbability;
        int currGeneration = 0;

        private void LaunchNSGA2()
        {
            crossoverProbability = Convert.ToDouble(txtCrossoverProb.Text);
            mutationProbability = Convert.ToDouble(txtMutationProb.Text);
            populationSize = Convert.ToInt32(txtPopSize.Text);
            var maxGenerations = Convert.ToInt32(txtGenerationsNr.Text);

            var path = Path.GetFullPath(PathToPsatSim);

            List<psatsim> configs = new List<psatsim>();
            for (int i = 0; i < populationSize; i++)
            {
                configs.Add(psatsim.GetRandomPsatsim());
                configs[i].config.name = $"Generation: {currGeneration}, Individual {i + 1}";
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

                var sortedPop = FastNonDominatedSort(population);
                foreach (var front in sortedPop)
                {
                    CalculateCrowdingDistance(front);
                }

                population = GenerateNextGeneration(population);
            } while (currGeneration < maxGenerations);
        }

        private List<List<Individual>> FastNonDominatedSort(List<Individual> population)
        {
            List<List<int>> dominatedIndividuals = new List<List<int>>(population.Count);
            List<List<int>> fronts = new List<List<int>> { new List<int>() };
            List<int> dominatedCount = new List<int>(new int[population.Count]);
            List<int> rank = new List<int>(new int[population.Count]);

            for (int i = 0; i < population.Count; i++)
            {
                dominatedIndividuals.Add(new List<int>());
                dominatedCount.Add(0);

                for (int j = 0; j < population.Count; j++)
                {

                    if (population[i].Dominates(population[j]))
                    {
                        if (!dominatedIndividuals[i].Contains(j))
                        {
                            dominatedIndividuals[i].Add(j);
                        }
                    }
                    else if (population[j].Dominates(population[i]))
                    {
                        dominatedCount[i]++;
                    }
                }

                if (dominatedCount[i] == 0)
                {
                    rank[i] = 0;
                    if (!fronts[0].Contains(i))
                    {
                        fronts[0].Add(i);
                    }
                }
            }

            int idx = 0;
            while (fronts[idx].Any())
            {
                List<int> nextFront = new List<int>();
                foreach (int dominatingIndividual in fronts[idx])
                {
                    foreach (int dominatedIndividual in dominatedIndividuals[dominatingIndividual])
                    {
                        dominatedCount[dominatedIndividual]--;
                        if (dominatedCount[dominatedIndividual] == 0)
                        {
                            rank[dominatedIndividual] = idx + 1;
                            if (!nextFront.Contains(dominatedIndividual))
                            {
                                nextFront.Add(dominatedIndividual);
                            }
                        }
                    }
                }

                idx++;
                fronts.Add(nextFront);
            }

            fronts.RemoveAt(fronts.Count - 1);
            var individualFronts = new List<List<Individual>>();
            foreach (var front1 in fronts)
            {
                var front2 = new List<Individual>();
                foreach (var ind in front1)
                {
                    front2.Add(population[ind]);
                    population[ind].Rank = rank[ind];
                }
                individualFronts.Add(front2);
            }
            return individualFronts;
        }

        private List<List<Individual>> SortPopulation(List<Individual> population)
        {
            List<List<Individual>> fronts = new List<List<Individual>>();
            List<Individual> currentFront = new List<Individual>(population);

            while (currentFront.Count > 0)
            {
                List<Individual> nextFront = new List<Individual>();

                foreach (var individual in currentFront)
                {
                    individual.DominatedCount = 0;
                    individual.DominatingIndividuals.Clear();

                    foreach (var other in currentFront)
                    {
                        if (individual.Dominates(other))
                            individual.DominatingIndividuals.Add(other);
                        else if (other.Dominates(individual))
                            individual.DominatedCount++;
                    }

                    if (individual.DominatedCount == 0)
                    {
                        nextFront.Add(individual);
                    }
                }

                fronts.Add(nextFront);

                List<Individual> individualsToRemove = new List<Individual>();

                foreach (var individual in nextFront)
                {
                    foreach (var dominatedIndividual in individual.DominatingIndividuals)
                    {
                        dominatedIndividual.DominatedCount--;

                        if (dominatedIndividual.DominatedCount == 0)
                        {
                            individualsToRemove.Add(dominatedIndividual);
                        }
                    }
                }

                // Remove dominated individuals outside the loop
                foreach (var toRemove in individualsToRemove)
                {
                    currentFront.Remove(toRemove);
                }
            }

            return fronts;
        }


        private void CalculateCrowdingDistance(List<Individual> front)
        {
            int size = front.Count;

            foreach (var ind in front)
            {
                ind.CrowdingDistance = 0;
            }

            for (int objIndex = 0; objIndex < front[0].Objectives.Length; objIndex++)
            {
                front = front.OrderBy(ind => ind.Objectives[objIndex]).ToList();

                front[0].CrowdingDistance = front[size - 1].CrowdingDistance = double.PositiveInfinity;

                for (int i = 1; i < size - 1; i++)
                {
                    front[i].CrowdingDistance += front[i + 1].Objectives[objIndex] - front[i - 1].Objectives[objIndex];
                }
            }
        }


        private Individual SelectByRankAndCrowding(Individual ind1, Individual ind2)
        {
            if (ind1.Rank > ind2.Rank)
                return ind1;
            else if (ind1.Rank < ind2.Rank)
                return ind2;
            else
                if (ind1.CrowdingDistance > ind2.CrowdingDistance)
                return ind1;
            else
                return ind2;
        }

        private List<Individual> GenerateNextGeneration(List<Individual> population)
        {
            List<Individual> nextGeneration = new List<Individual>();
            var newIndividuals = 0;
            while (nextGeneration.Count < population.Count)
            {
                GetParents(population, out Individual parent1, out Individual parent2);
                nextGeneration.Add(parent1);
                nextGeneration.Add(parent2);

                var child1 = parent1.DeepCopy();
                var child2 = parent2.DeepCopy();
                var changed = false;
                if (RandomProbability() < crossoverProbability)
                {
                    child1.Config.CrossoverRandomVariables(child2.Config, Convert.ToDouble(txtCrossoverDistance.Text));
                    child1.Config.config.name = $"Generation: {currGeneration}, Individual {newIndividuals + 1}";
                    child2.Config.config.name = $"Generation: {currGeneration}, Individual {newIndividuals + 2}";
                    child1.UpToDate = false;
                    child2.UpToDate = false;
                    newIndividuals += 2;
                    changed = true;
                }

                if (RandomProbability() < mutationProbability)
                {
                    MutateChild(ref newIndividuals, ref child1, changed);
                }
                if (RandomProbability() < mutationProbability)
                {
                    MutateChild(ref newIndividuals, ref child2, changed);
                }

                if (!child1.UpToDate)
                    nextGeneration.Add(child1);
                if (!child2.UpToDate)
                    nextGeneration.Add(child2);
            }
            return nextGeneration;

            void MutateChild(ref int newIndividuals, ref Individual child, bool changed)
            {
                child.Config.MutateRandomVariable(Convert.ToDouble(txtMutationDistance.Text));
                if (!changed)
                {
                    child.Config.config.name = $"Generation: {currGeneration}, Individual {newIndividuals + 1}";
                    child.UpToDate = false;
                    newIndividuals += 1;
                }
            }
        }

        private void GetParents(List<Individual> population, out Individual parent1, out Individual parent2)
        {
            var random = new Random();
            var parent1Index = random.Next(0, population.Count);
            var parent2Index = random.Next(0, population.Count);
            var possibleParent1 = population[parent1Index];
            var possibleParent2 = population[parent2Index];
            parent1 = SelectByRankAndCrowding(possibleParent1, possibleParent2);
            parent1Index = random.Next(0, population.Count);
            parent2Index = random.Next(0, population.Count);
            possibleParent1 = population[parent1Index];
            possibleParent2 = population[parent2Index];
            parent2 = SelectByRankAndCrowding(possibleParent1, possibleParent2);
        }

        private double RandomProbability()
        {
            Random random = new Random();
            return random.NextDouble();
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
            LaunchNSGA2();
        }
    }

    class Individual
    {
        public psatsim Config;
        public double[] Objectives = new double[2];
        public double Fitness;
        public bool UpToDate = false;

        public Individual(psatsim config, double avgCpi, double avgEnergy)
        {
            this.Config = config;
            this.Objectives[0] = avgCpi;
            this.Objectives[1] = avgEnergy;
            UpToDate = true;
        }

        public int Rank;
        public double CrowdingDistance;
        public int DominatedCount;
        public List<Individual> DominatingIndividuals = new List<Individual>();

        public bool Dominates(Individual other)
        {
            const double epsilon = 1e-10;

            return this.Objectives[0] <= other.Objectives[0] + epsilon && this.Objectives[1] <= other.Objectives[1] + epsilon
                && (this.Objectives[0] + epsilon < other.Objectives[0] || this.Objectives[1] + epsilon < other.Objectives[1]);
        }

        public Individual DeepCopy()
        {
            var copiedIndividual = this.MemberwiseClone() as Individual;
            return copiedIndividual;
        }
    }


}

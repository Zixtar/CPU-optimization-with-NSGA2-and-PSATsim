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

        public void MutateRandomVariable(double distance)
        {
            var rand = new Random();
            var variableToChange = (psatsimMutation)rand.Next(0, Enum.GetValues<psatsimMutation>().Length);

            switch (variableToChange)
            {
                case psatsimMutation.executionInteger:
                    config.execution.CreateMutatedInteger(distance);
                    break;
                case psatsimMutation.executionFloating:
                    config.execution.CreateMutatedFloating(distance);
                    break;
                case psatsimMutation.executionBranch:
                    config.execution.CreateMutatedBranch(distance);
                    break;
                case psatsimMutation.executionMemory:
                    config.execution.CreateMutatedMemory(distance);
                    break;
                case psatsimMutation.generalSuperscalar:
                    config.general[0].CreateMutatedSuperscalar(distance);
                    break;
                case psatsimMutation.generalRename:
                    config.general[0].CreateMutatedRename(distance);
                    break;
                case psatsimMutation.generalReorder:
                    config.general[0].CreateMutatedReorder(distance);
                    break;
                case psatsimMutation.generalRsbArch:
                    config.general[0].CreateMutatedRsbArch(distance);
                    break;
                case psatsimMutation.generalVdd:
                    config.general[0].CreateMutatedFrequency(distance);
                    break;
                case psatsimMutation.generalRsPerRsb:
                    config.general[0].CreateMutatedRsPerRsb(distance);
                    break;
                case psatsimMutation.generalFrequency:
                    config.general[0].CreateMutatedFrequency(distance);
                    break;
                case psatsimMutation.generalSeparateDispatch:
                    config.general[0].CreateMutatedSeparateDispatch(distance);
                    break;
                default:
                    break;
            }
        }

        internal void CrossoverRandomVariables(psatsim psatsimVar2, double distance)
        {
            var rand = new Random();
            var numberOfVariablesToChange = Enum.GetValues<psatsimMutation>().Length * distance;
            List<psatsimMutation> changedVariables = new List<psatsimMutation>();
            
            while (numberOfVariablesToChange > 0)
            {
                psatsimMutation variableToCrossover;
                do
                {
                    variableToCrossover = (psatsimMutation)rand.Next(0, Enum.GetValues<psatsimMutation>().Length);
                } while (changedVariables.Contains(variableToCrossover));
                changedVariables.Add(variableToCrossover);
                numberOfVariablesToChange--;

                switch (variableToCrossover)
                {
                    case psatsimMutation.executionInteger:
                        {
                            var temp = config.execution.integer;
                            config.execution.integer = psatsimVar2.config.execution.integer;
                            psatsimVar2.config.execution.integer = temp;
                            break;
                        }
                    case psatsimMutation.executionFloating:
                        {
                            var temp = config.execution.floating;
                            config.execution.floating = psatsimVar2.config.execution.floating;
                            psatsimVar2.config.execution.floating = temp;
                            break;
                        }
                    case psatsimMutation.executionBranch:
                        {
                            var temp = config.execution.branch;
                            config.execution.branch = psatsimVar2.config.execution.branch;
                            psatsimVar2.config.execution.branch = temp;
                            break;
                        }
                    case psatsimMutation.executionMemory:
                        {
                            var temp = config.execution.memory;
                            config.execution.memory = psatsimVar2.config.execution.memory;
                            psatsimVar2.config.execution.memory = temp;
                            break;
                        }
                    case psatsimMutation.generalSuperscalar:
                        {
                            var temp = config.general[0].superscalar;
                            config.general[0].superscalar = psatsimVar2.config.general[0].superscalar;
                            psatsimVar2.config.general[0].superscalar = temp;
                            break;
                        }
                    case psatsimMutation.generalRename:
                        {
                            var temp = config.general[0].rename;
                            config.general[0].rename = psatsimVar2.config.general[0].rename;
                            psatsimVar2.config.general[0].rename = temp;
                            break;
                        }
                    case psatsimMutation.generalReorder:
                        {
                            var temp = config.general[0].reorder;
                            config.general[0].reorder = psatsimVar2.config.general[0].reorder;
                            psatsimVar2.config.general[0].reorder = temp;
                            break;
                        }
                    case psatsimMutation.generalRsbArch:
                        {
                            var temp = config.general[0].rsb_architecture;
                            config.general[0].rsb_architecture = psatsimVar2.config.general[0].rsb_architecture;
                            psatsimVar2.config.general[0].rsb_architecture = temp;
                            break;
                        }
                    case psatsimMutation.generalVdd:
                        {
                            var temp = config.general[0].vdd;
                            config.general[0].vdd = psatsimVar2.config.general[0].vdd;
                            psatsimVar2.config.general[0].vdd = temp;
                        }
                        break;
                    case psatsimMutation.generalRsPerRsb:
                        {
                            var temp = config.general[0].rs_per_rsb;
                            config.general[0].rs_per_rsb = psatsimVar2.config.general[0].rs_per_rsb;
                            psatsimVar2.config.general[0].rs_per_rsb = temp;
                        }
                        break;
                    case psatsimMutation.generalFrequency:
                        {
                            var temp = config.general[0].frequency;
                            config.general[0].frequency = psatsimVar2.config.general[0].frequency;
                            psatsimVar2.config.general[0].frequency = temp;
                        }
                        break;
                    case psatsimMutation.generalSeparateDispatch:
                        {
                            var temp = config.general[0].separate_dispatch;
                            config.general[0].separate_dispatch = psatsimVar2.config.general[0].separate_dispatch;
                            psatsimVar2.config.general[0].separate_dispatch = temp;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        internal enum psatsimMutation
        {
            executionInteger,
            executionFloating,
            executionBranch,
            executionMemory,
            generalSuperscalar,
            generalRename,
            generalReorder,
            generalRsbArch,
            generalVdd,
            generalRsPerRsb,
            generalFrequency,
            generalSeparateDispatch,
        }

        [XmlElement]
        public config config;
    }
}

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
            var variableToChange = (psatsimMutation)rand.Next(1, Enum.GetValues<psatsimMutation>().Length);

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

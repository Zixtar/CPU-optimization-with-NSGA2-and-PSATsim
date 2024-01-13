using MicroControllerOptimizer.XMLSerialization;
using System.Collections.Generic;

namespace MicroControllerOptimizer
{
    public class Individual
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

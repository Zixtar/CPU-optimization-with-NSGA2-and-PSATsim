using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroControllerOptimizer
{
    public class NsgaII
    {
        public double crossoverProbability;
        public double mutationProbability;
        public double mutationDistance;
        public double crossoverDistance;
        public int currGeneration;
        public List<List<Individual>> FastNonDominatedSort(List<Individual> population)
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

        public void CalculateCrowdingDistance(List<Individual> front)
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


        public Individual SelectByRankAndCrowding(Individual ind1, Individual ind2)
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

        public List<Individual> GenerateNextGeneration(List<Individual> population)
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
                    child1.Config.CrossoverRandomVariables(child2.Config, crossoverDistance);
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
                child.Config.MutateRandomVariable(mutationDistance);
                if (!changed)
                {
                    child.Config.config.name = $"Generation: {currGeneration}, Individual {newIndividuals + 1}";
                    child.UpToDate = false;
                    newIndividuals += 1;
                }
            }
        }

        public void GetParents(List<Individual> population, out Individual parent1, out Individual parent2)
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

    }


}

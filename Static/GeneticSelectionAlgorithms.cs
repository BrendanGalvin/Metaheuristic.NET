using Metaheuristic.NET.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaheuristic.NET.Static
{
    /// <summary>
    /// A collection of selection algorithms that will return a <see cref="Dictionary{IGeneticChromosome, Double}"/> of your IGeneticChromosome implementation as the key, and the chance of it being selected as a parent in a crossover algorithm as the value.
    /// </summary>
    public static class GeneticSelectionAlgorithms
    {
        /// <summary>
        /// In Selection algorithms where only a limited number of parents is desired, this is the upper bound.
        /// </summary>
        public static int NumberOfParentsToReturn = 2;

        /// <summary>
        /// Returns the top parents from the population, based on their fitness scores, each with equal chances of being selected. The number of parents returns is based on <see cref="NumberOfParentsToReturn"/>.
        /// </summary>
        /// <param name="CurrentGeneration">The population to draw from.</param>
        /// <returns></returns>
        public static async Task<Dictionary<IGeneticChromosome, double>> TopNParents(List<IGeneticChromosome> CurrentGeneration)
        {
            if (CurrentGeneration == null || CurrentGeneration.Count == 0)
                return new Dictionary<IGeneticChromosome, double>(); // No current generation specified; don't do anything.
            for (int i = 0; i < CurrentGeneration.Count; i++)
            {
                await CurrentGeneration[i].RefreshFitnessScore(); // Calculate fitness scores of population under examination
            }
            // We basically just want to return the top N parents based on fitness scores, with equal probabilities of being chosen.
            // We'll also return them in the order of highest fitness to least fitness, just in case that ever matters (though technically this additional operation adds a small amount of overhead).
            return CurrentGeneration.OrderByDescending(x => x.FitnessScore).Take(NumberOfParentsToReturn).ToDictionary(keySelector: g => g, elementSelector: y => 1.0 / NumberOfParentsToReturn);
        }


        /// <summary>
        /// Returns all parents with their percentages based on proportional ranking - for instance, in a population of 4 chromosomes, the top performer will have 4 "draws" to be chosen, the second best will have 3 "draws" to be chosen, etc., while the lowest performer has 1 "draw" to be chosen as a parent in any given crossover/breeding for subsequent generations.
        /// <para>This helps to prevent early convergence on local maxima by ensuring that the best early-performing candidates don't become over-represented immediately.</para>
        /// </summary>
        /// <param name="CurrentGeneration">The population to draw from.</param>
        /// <returns></returns>
        public static async Task<Dictionary<IGeneticChromosome, double>> Rank(List<IGeneticChromosome> CurrentGeneration)
        {
            Dictionary<IGeneticChromosome, double> ParentPopulation = new Dictionary<IGeneticChromosome, double>();
            if (CurrentGeneration == null || CurrentGeneration.Count == 0)
                return ParentPopulation; // No current generation specified; don't do anything.
            for (int i = 0; i < CurrentGeneration.Count; i++)
            {
                await CurrentGeneration[i].RefreshFitnessScore(); // Calculate fitness scores of population under examination
            }
            var SortedGeneration = CurrentGeneration.OrderByDescending(x => x.FitnessScore).ToList();
            double TotalNumberOfDraws = SortedGeneration.Count * 0.5 * (SortedGeneration.Count + 1); // Treating this as a combinatorics problem first, we simplify the finite series
            for (int i = 0; i < SortedGeneration.Count; i++)
            {
                // So the top chromosome in a population of 10, has 10 "draws" from the total population, which equates to 10 / 55 or just under a 20% chance.
                ParentPopulation.Add(SortedGeneration[i], SortedGeneration.Count - i / TotalNumberOfDraws);
            }
            return ParentPopulation;
        }

        /// <summary>
        /// Returns the parents and their percentage chance of being selected based on the concept of a roulette wheel where the size of an individual parents' section of the wheel, is proportional to the magnitude of its fitness in relation to the total fitness of all parents.
        /// <para>This differs from <see cref="Rank(List{IGeneticChromosome})"/> in that it is possible for one extremely high performing candidate to crowd out all other candidates in the population, or for extremely low performing candidates to essentially have zero chance of being selected, since this method is based on the magnitude of their fitness, not merely their rank amidst their peers.</para>
        /// <para>A well known criticism of this selection method is that it can lead to converging on local maxima due to crowding out alternate chromosomes too quickly if one or two chromosomes become high performers early on.</para>
        /// </summary>
        /// <param name="CurrentGeneration"></param>
        /// <returns></returns>
        public static async Task<Dictionary<IGeneticChromosome, double>> Roulette(List<IGeneticChromosome> CurrentGeneration)
        {
            Dictionary<IGeneticChromosome, double> ParentPopulation = new Dictionary<IGeneticChromosome, double>();
            if (CurrentGeneration == null || CurrentGeneration.Count == 0)
                return ParentPopulation; // No current generation specified; don't do anything.
            double TotalFitness = 0;
            for (int i = 0; i < CurrentGeneration.Count; i++)
            {
                await CurrentGeneration[i].RefreshFitnessScore(); // Calculate fitness scores of population under examination
                TotalFitness += CurrentGeneration[i].FitnessScore; // Add to the total
            }
            var SortedGeneration = CurrentGeneration.OrderByDescending(x => x.FitnessScore).ToList();
            for (int i = 0; i < SortedGeneration.Count; i++)
            {
                // So the top chromosome in a population of 10, has 10 "draws" from the total population, which equates to 10 / 55 or just under a 20% chance.
                ParentPopulation.Add(SortedGeneration[i], SortedGeneration[i].FitnessScore / TotalFitness);
            }
            return ParentPopulation;
        }

    }
}

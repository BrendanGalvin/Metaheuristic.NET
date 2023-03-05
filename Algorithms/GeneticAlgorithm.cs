using Metaheuristic.NET.Interfaces;
using Metaheuristic.NET.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaheuristic.NET.Algorithms
{
    /// <summary>
    /// A Genetic Algorithm ("GA") is a metaheuristic multi-population search algorithm that has 4 basic requirements or tasks to be properly defined as a GA.
    /// <para>First, a GA must have a problem space that is implemented and ready for study. This essentially means you must have your own object ready to be used in the GA, and this means properly implementing the <see cref="IGeneticChromosome"/> interface in your desired object.</para>
    /// <para>Second, a GA must be told how to initialize a population of such chromosomes - the initial population may be determined by some random mechanism or may be manually supplied. In either case, in this GA, you must supply the initial starting population yourself.</para>
    /// <para>Third, you must supply a selection mechanism, which informs the GA how it should select the chromosomes fit for reproduction from its population. Selection mechanisms are classes that properly implement <see cref="IGeneticSelection"/></para>
    /// <para>Lastly, a GA must have an ability to determine the fitness level, or "score", of already located chromosomes (or "solutions"). This fitness function is part of the <see cref="IGeneticChromosome"/> interface, specifically <see cref="IGeneticChromosome.FitnessScore"/> that your objects should inherit if they are to be used in this GA class.</para>
    /// <para>A GA must also have control parameters set, including the <see cref="MutationFactor"/> for value encoded genes, <see cref="MutationChance"/> for the chance that a given gene will be mutated, </para>
    /// </summary>
    public class GeneticAlgorithm
    {
        /// <summary>
        /// The constructor for a new instance of the Genetic Algorithm class.
        /// </summary>
        /// <param name="StartingGen">The initial population to start with.</param>
        /// <param name="PopulationSize">The maximum population size to produce in subsequent generations (this does NOT need to be smaller or equal to the initial population size - the two are completely unrelated.)</param>
        /// <param name="MutationChance">The fractional chance (between 0 and 1) that any given gene in a chromosome will mutate when a new population is bred.</param>
        /// <param name="MutationFactor">The proportion (between 0 and 1) that a mutation may alter a value-encoded gene by. For instance, a value gene of 86, with a MutationFactor of 0.2, can be reduced or increased by 0 to 20% of its current value (or in this hypothetical, from 0 to 17.2, pseudo-randomly determined).</param>
        public GeneticAlgorithm(List<IGeneticChromosome> StartingGen, Func<List<IGeneticChromosome>, Task<Dictionary<IGeneticChromosome, double>>> Selection, Func<Dictionary<IGeneticChromosome, double>, IGeneticChromosome> Crossover, int PopulationSize = 100, double MutationChance = 0.2, double MutationFactor = 3)
        {
            CurrentGeneration = StartingGen;
            this.PopulationSize = PopulationSize;
            this.MutationChance = MutationChance;
            this.MaximumMutationFactor = MutationFactor;
            this.Selection = Selection;
            this.Crossover = Crossover;
        }
        /// <summary>
        /// The current population of chromosomes (a chromosome being a collection of "genes", or parameters, that govern how another object/class/algorithm behaves, depending on the implementation). In other words, this is the current list of objects being operated on by the GA.
        /// </summary>
        List<IGeneticChromosome> CurrentGeneration { get; set; }
        /// <summary>
        /// A list containing, in chronological order, all generations that have been computed during the current runtime of this genetic algorithm.
        /// </summary>
        List<List<IGeneticChromosome>> Generations { get; set; }
        /// <summary>
        /// The amount of chromosomes to breed in each successive generation.
        /// </summary>
        int PopulationSize { get; set; }
        /// <summary>
        /// The chance (on a scale of 0 to 1) that any given gene will mutate to a different value than the inherited value from its parents.
        /// </summary>
        double MutationChance { get; set; }
        /// <summary>
        /// For value encoded chromosomes, this determines how much a given chromosome should mutate, if it is selected for mutation at all, as a proportion of its current value (i.e. a MaximumMutationFactor of 0.2 will allow a mutating gene to either increase or decrease by 0 to 20% of its current pre-mutation value).
        /// </summary>
        double MaximumMutationFactor { get; set; }
        Func<List<IGeneticChromosome>, Task<Dictionary<IGeneticChromosome, double>>> Selection;
        Func<Dictionary<IGeneticChromosome, double>, IGeneticChromosome> Crossover;

        /// <summary>
        /// An implementation of a genetic algorithm that will breed a new set of objects. Time complexity: O(n*m) where n is the size of the population, and m is the size of the genome.
        /// </summary>
        /// <returns></returns>
        public async Task BreedNewGeneration()
        {
            var Parents = await Selection(CurrentGeneration); // Get the highest-fitness parents.
            if (Parents == null)
                return; // No parents, usually means there's no current generation.
            var genPlaceholder = new List<IGeneticChromosome>();
            for (int i = 0; i < CurrentGeneration.Count; i++) // We want to clone and archive the current population
            {
                genPlaceholder.Add(CurrentGeneration[i].Clone());
            }
            Generations.Add(genPlaceholder); // Archive the clone of the current generation
            List<IGeneticChromosome> NewGeneration = new List<IGeneticChromosome>();
            NewGeneration.Add(Crossover(Parents)); // Genetic crossover - this particular algorithm uses Uniform Crossover, so each gene is randomly grabbed from a parent at the time of crossover.
            Mutation(NewGeneration); // Apply mutations to the new population.

            for (int i = 0; i < CurrentGeneration.Count; i++)
            {
                // Now we want to set the Current Generation's genomes to the new values.
                // We do this instead of just setting CurrentGeneration to NewGeneration, because we want this engine to be generic and applicable to theoretically any
                // number of algorithms or problems, so to keep the genetic algorithm separated from the algorithm implementation it's evolving,
                // the algorithm implementation itself will update its values internally based on the new genome values - that way the genetic algorithm doesn't have to know
                // how to directly modify anything it's being used to evolve.
                CurrentGeneration[i].DecimalGenes = NewGeneration[i].DecimalGenes;
            }
        }



        private void Mutation(List<IGeneticChromosome> GenerationToMutate)
        {
            Random rnd = new Random();
            for (int i = 0; i < GenerationToMutate.Count; i++)
            {
                var genome = GenerationToMutate[i]; // Iterate over and grab each genome
                for (int j = 0; j < genome.DecimalGenes.Count; j++)
                {
                    // One concern here is how large the generations and genomes are.
                    // If we have a generation of 150 genomes, and each genome has 15 genes to evolve,
                    // then we're doing 2,250 loops in this method alone.
                    // That doesn't even count the other methods, which also do the same.
                    double mutation = rnd.NextDouble();
                    if (mutation < MutationChance) // Uh oh, we mutated! We're an X-Man now
                    {
                        int addition = rnd.Next(0, 2); // Are we subtracting/dividing, or adding/multiplying?
                        double multiplicationFactor = rnd.NextDouble() / MaximumMutationFactor; // The way we use this, essentially we're saying "some value between 0% and x%" for mutating the genome. If MutationFactor is 3, then 33%; if it's 5, then 20%; etc. etc.
                        var gene = genome.DecimalGenes[j];
                        if (gene % 1 == 0) // It's an integer.
                        {
                            if (addition == 0)
                                genome.DecimalGenes[j] -= Math.Ceiling(genome.DecimalGenes[j] * Convert.ToDecimal(multiplicationFactor)); // Minimum 1, maximum x% of value
                            else
                                genome.DecimalGenes[j] += Math.Ceiling(genome.DecimalGenes[j] * Convert.ToDecimal(multiplicationFactor)); // Minimum 1, maximum x% of value
                        }
                        else
                        {
                            if (addition == 0)
                                genome.DecimalGenes[j] *= (1 - Convert.ToDecimal(multiplicationFactor)); // Decrease number by 0-x%
                            else
                                genome.DecimalGenes[j] *= (1 + Convert.ToDecimal(multiplicationFactor)); // Increase number by 0+x%
                        }
                    }
                }
            }
        }
    }
}

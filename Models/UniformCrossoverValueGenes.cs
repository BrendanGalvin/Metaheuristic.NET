using Metaheuristic.NET.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaheuristic.NET.Models
{
    /// <summary>
    /// A genetic algorithm that can be used to evolve any objects that inherit IGenetic.
    /// <para>This genetic algorithm uses Value Encoding for genes, and Uniform Crossover for the crossover operation.</para>
    /// </summary>
    public class UniformCrossoverValueGenes
    {

        public UniformCrossoverValueGenes(List<IGeneticChromosome> StartingGen, int NumOfParents = 2, double MutationChance = 0.2, double MutationFactor = 3, int SizeOfGeneration = 100)
        {
            CurrentGeneration = StartingGen;
            NumberOfParents = NumOfParents;
            this.MutationChance = MutationChance;
            this.MutationFactor = MutationFactor;
            this.SizeOfGeneration = SizeOfGeneration;
        }
        /// <summary>
        /// The current population of chromosomes (i.e. current genes being used to try and solve the given problem).
        /// </summary>
        List<IGeneticChromosome> CurrentGeneration { get; set; }
        /// <summary>
        /// A list containing, in chronological order, all generations that have been computed during the current runtime of this genetic algorithm.
        /// </summary>
        List<List<IGeneticChromosome>> Generations { get; set; } = new List<List<IGeneticChromosome>>();
        /// <summary>
        /// The number of top fitness candidates to pull from the population, when selecting new parents to breed for subsequent generations.
        /// </summary>
        int NumberOfParents { get; set; }
        /// <summary>
        /// The chance (on a scale of 0 to 1) that any given gene will mutate to a different value than the inherited value from its parents.
        /// </summary>
        double MutationChance { get; set; }
        /// <summary>
        /// The divisor for 1, to determine the maximum percentage that random mutations can alter a gene (for instance, if set to 5, then 1/5 or 20% is the maximum they can increase or decrease, and that number is randomly determined upon mutation.)
        /// </summary>
        double MutationFactor { get; set; }
        /// <summary>
        /// The maximum size of a generation that will be bred; for instance if it is set to 100, then a maximum of 100 new chromosomes will be bred per generation.
        /// </summary>
        int SizeOfGeneration { get; set; }

        /// <summary>
        /// An implementation of a genetic algorithm that will breed a new set of objects. Time complexity: O(n*m) where n is the size of the population, and m is the size of the genome.
        /// </summary>
        /// <returns></returns>
        public async Task BreedNewGeneration()
        {
            var Parents = await Selection(); // Get the highest-fitness parents.
            if (Parents == null)
                return; // No parents, usually means there's no current generation.
            var genPlaceholder = new List<IGeneticChromosome>();
            for (int i = 0; i < CurrentGeneration.Count; i++) // We want to clone and archive the current population
            {
                genPlaceholder.Add(CurrentGeneration[i].Clone());
            }
            Generations.Add(genPlaceholder); // Archive the clone of the current generation
            var NewGeneration = await Crossover(Parents); // Genetic crossover - this particular algorithm uses Uniform Crossover, so each gene is randomly grabbed from a parent at the time of crossover.
            await Mutation(NewGeneration); // Apply mutations to the new population.

            for (int i = 0; i < CurrentGeneration.Count; i++)
            {
                // Now we want to set the Current Generation's genomes to the new values.
                // We do this instead of just setting CurrentGeneration to NewGeneration, because we want this engine to be generic and applicable to theoretically any
                // number of algorithms or problems, so to keep the genetic algorithm separated from the algorithm implementation it's evolving,
                // the algorithm implementation itself will update its values internally based on the new genome values - that way the genetic algorithm doesn't have to know
                // how to directly modify anything it's being used to evolve.
                CurrentGeneration[i].DecimalGenes = NewGeneration[i].DecimalGenes;
            }
            for (int i = CurrentGeneration.Count; i < NewGeneration.Count; i++)
            {
                // If we have more chromosomes in the new generation, we just want to add them to the current generation.
                CurrentGeneration.Add(NewGeneration[i].Clone());
            }
        }

        /// <summary>
        /// Selects the top n parents from the population.
        /// </summary>
        /// <returns></returns>
        private async Task<List<IGeneticChromosome>> Selection()
        {
            if (CurrentGeneration == null || CurrentGeneration.Count == 0)
                return new List<IGeneticChromosome>(); // No current generation specified; don't do anything.
            for (int i = 0; i < CurrentGeneration.Count; i++)
            {
                await CurrentGeneration[i].RefreshFitnessScore();
            }
            return CurrentGeneration.OrderByDescending(x => x.FitnessScore).Take(NumberOfParents).ToList();
        }

        /// <summary>
        /// Crosses the genes of the parents to produce the new generation of children.
        /// </summary>
        /// <param name="Parents"></param>
        /// <returns></returns>
        private async Task<List<IGeneticChromosome>> Crossover(List<IGeneticChromosome> Parents)
        {
            Random rnd = new Random();
            var newGeneration = new List<IGeneticChromosome>();
            for(int i = 0; i < SizeOfGeneration; i++)
            {
                newGeneration.Add(CurrentGeneration[0].Factory()); // It doesn't matter, as far as the Genetic Algorithm is concerned, which one you pick, because the genes are all going to be overridden by the parents anyway.
                for (int j = 0; j < newGeneration[i].DecimalGenes.Count; j++) // Time to collect genes from parents!
                {
                    IGeneticChromosome parent = Parents[rnd.Next(0, Parents.Count)]; // Get a random parent
                    newGeneration[i].DecimalGenes[j] = parent.DecimalGenes[j]; // Acquire gene
                }
            }
            return newGeneration;

        }

        private async Task Mutation(List<IGeneticChromosome> GenerationToMutate)
        {
            Random rnd = new Random();
            for (int i = 0; i < GenerationToMutate.Count; i++)
            {
                var genome = GenerationToMutate[i]; // Iterate over and grab each genome
                for(int j = 0; j < genome.DecimalGenes.Count; j++)
                {
                    // One concern here is how large the generations and genomes are.
                    // If we have a generation of 150 genomes, and each genome has 15 genes to evolve,
                    // then we're doing 2,250 loops in this method alone.
                    // That doesn't even count the other methods, which also do the same.
                    double mutation = rnd.NextDouble();
                    if(mutation < MutationChance) // Uh oh, we mutated! We're an X-Man now
                    {
                        int addition = rnd.Next(0, 2); // Are we subtracting/dividing, or adding/multiplying?
                        double multiplicationFactor = rnd.NextDouble() / MutationFactor; // The way we use this, essentially we're saying "some value between 0% and x%" for mutating the genome. If MutationFactor is 3, then 33%; if it's 5, then 20%; etc. etc.
                        var gene = genome.DecimalGenes[j];
                        if (gene % 1 == 0) // It's an integer.
                        {
                            if(addition == 0)
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

using Metaheuristic.NET.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaheuristic.NET.Static
{
    public static class GeneticCrossoverAlgorithms
    {

        /// <summary>
        /// Given a list of parents, this will produce a new instance of the same Type as the Parents, but with a new genome that is the result of uniform crossover between the parents.
        /// <para>This will perform the crossover from 2 parents selected according to the percentages in the Dictionary (for instance, if you have 5 parents, it will select 2 parents at random according to their weights in the values of the Dictionary, and use them for the crossover; the other 3 candidate parents, whichever those are, will effectively be irrelevant.)</para>
        /// </summary>
        /// <param name="Parents"></param>
        /// <returns></returns>
        public static IGeneticChromosome Uniform2ParentCrossover(Dictionary<IGeneticChromosome, double> Parents)
        {
            if (Parents.Count < 2)
                throw new Exception("You must have 2 parents for the Metaheuristic.NET.Static.Uniform2ParentCrossover crossover method.");
            Random rnd = new Random();
            IGeneticChromosome newChromosome = Parents.First().Key.Factory();
            double FirstRoll = rnd.NextDouble();
            IGeneticChromosome Parent1 = null;
            IGeneticChromosome Parent2 = null;
            // Let's say you have:
            // Parent1 -> 0.4
            // Parent2 -> 0.3
            // Parent3 -> 0.1
            // Parent4 -> 0.1
            // Parent5 -> 0.1

            // We could basically iterate over the parents, deducting their value (AKA their probability) from a roll, and if the roll then goes negative, that's the parent we want
            // (because that means the roll was effectively within that parent's success range in the probability distribution)
            // For instance, rolling 0.45, would put us out of range of Parent1 (we would deduct 0.4 from the roll, giving us 0.05), but would land us in Parent2's range
            // (which we can tell because we deduct 0.3 and arrive at -0.25)
            foreach (var Parent in Parents)
            {
                FirstRoll -= Parent.Value;
                if(FirstRoll < 0)
                {
                    Parent1 = Parent.Key;
                    break;
                }
            }
            // Now we want to roll for the next parent, ensuring we don't roll the same parent again.
            while (true)
            {
                double SecondRoll = rnd.NextDouble();
                foreach (var Parent in Parents)
                {
                    SecondRoll -= Parent.Value;
                    if (SecondRoll < 0)
                    {
                        if (Parent1 != Parent.Key)
                        {
                            Parent2 = Parent.Key;
                            break;
                        }
                        else
                        {
                            // Don't pick the same parent!
                            break;
                        }
                    }
                }
                if(Parent2 != null)
                {
                    // If we've settled on a second parent, break out of the outer loop.
                    break;
                }
            }

            for (int j = 0; j < newChromosome.DecimalGenes.Count; j++) // Time to collect genes from parents!
            {
                var roll = rnd.Next(1, 3); // Either returns 1 or 2
                if (roll == 1)
                    newChromosome.DecimalGenes[j] = Parent1.DecimalGenes[j]; // Acquire gene
                else if (roll == 2)
                    newChromosome.DecimalGenes[j] = Parent2.DecimalGenes[j]; // Acquire gene
            }
            return newChromosome;
        }

    }
}

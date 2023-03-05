using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaheuristic.NET.Interfaces
{
    public interface IGeneticChromosome
    {
        /// <summary>
        /// The list of all Decimal properties this object uses that are considered genes suitable for mutating and altering through evolving the algorithm.
        /// <para>It is recommended you alter the "setter" and "getter" of this property to iterate over the object's actual properties that are not visible to this interface, and set or get them (in the same order) so that the genetic algorithm can safely overwrite an object's chromosome to enact actual changes to the object.</para>
        /// <para>For numeric properties that are not decimals, simply convert between decimal and the proper datatype. Decimal provides the greatest precision which is why it is used here.</para>
        /// </summary>
        List<decimal> DecimalGenes { get; set; }
        /// <summary>
        /// The fitness score representing how fit this object is - higher is better.
        /// </summary>
        public double FitnessScore { get; set; }
        /// <summary>
        /// Sets the object's own FitnessScore according to the local implementation defined by the object.
        /// </summary>
        /// <returns></returns>
        public Task RefreshFitnessScore();
        /// <summary>
        /// Returns a shallow copy of the current object. NOTE: BE SURE TO INSTANTIATE A NEW COPY OF THE OBJECT'S GENES, OTHERWISE YOU WILL RETURN A REFERENCE TO THE SAME LIST OF GENES AS THE ORIGINAL CHROMOSOME! That could cause serious instability (especially if archiving each chromosome between generations!)
        /// </summary>
        /// <returns></returns>
        public IGeneticChromosome Clone();
        /// <summary>
        /// Returns a new, blank-slate instance of the inheriting type, with an identical copy of the genes as to instanced type, similar to the <see cref="Clone"/> method (this is useful for abstracting the constructor of inheriting types away, while still giving us access, if we need/want, to the original chromosome examined, such as for certain special crossover operations).
        /// <para>Note that the key difference between this method, and the <see cref="Clone"/> method, is that this method returns not just a clone, but a clone of the inheriting type that possesses a per-value identical chromosome but otherwise has been wiped of any instance-specific data such as any data relating to its fitness score.</para>
        /// </summary>
        /// <returns></returns>
        public IGeneticChromosome Factory();
    }
}

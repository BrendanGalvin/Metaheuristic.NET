using Metaheuristic.NET.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaheuristic.NET.Interfaces
{
    public interface IGeneticSelection
    {
        public Task<List<IGeneticChromosome>> Selection();
    }
}

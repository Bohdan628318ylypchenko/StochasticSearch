using MathNet.Numerics.LinearAlgebra.Double;

namespace StochasticSearchLib
{
    public sealed class StochasticSearcher
    {
        /// <summary>
        /// Minimizes given function by Stochastic search.
        /// </summary>
        /// <param name="f"> Function to minimize </param>
        /// <param name="directionGenerationAtteptCount"> Sets count of direction generation attempts to try finding min before lowering radius. </param>
        /// <param name="minR"> Minimum search radius. </param>
        /// <param name="initR"> Initial search radius. </param>
        /// <param name="dR"> Radius decrement coefficient: nR = cR * dR </param>
        /// <returns> Minimization path as array of tuples. A: point, B: function value in point. </returns>
        public Tuple<Vector, double>[] MinimizeFunction(Func<Vector, double> f,
                                                        double directionGenerationAtteptCount,
                                                        double minR, double initR, double dR)
        {
            throw new NotImplementedException();
        }
    }
}

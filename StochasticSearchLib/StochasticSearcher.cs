using MathNet.Numerics.LinearAlgebra.Double;
using StochasticSearchLib.Optimization1D;

namespace StochasticSearchLib
{
    /// <summary>
    /// Utility class.
    /// Implements stochastic minimum search.
    /// </summary>
    public sealed class StochasticSearcher
    {
        private readonly ISearcher1D _searcher1D;
        private readonly Random _random;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="searcher1D"> ISearcher1D implementation to use
        ///                           during minimum search in direction.
        /// </param>
        /// <param name="seed"> Seed to initialize inner Random instance with. </param>
        public StochasticSearcher(ISearcher1D searcher1D, int seed)
        {
            _searcher1D = searcher1D;
            _random = new Random(seed);
        }

        /// <summary>
        /// Minimizes given function by Stochastic search.
        /// </summary>
        /// <param name="f"> Function to minimize </param>
        /// <param name="directionGenerationAttemptBeforeRadiusDecrementCount"> Sets count of direction generation attempts
        ///                                                                     to try finding min before lowering radius.
        /// </param>
        /// <param name="rMin"> Minimum search radius. </param>
        /// <param name="rInit"> Initial search radius. </param>
        /// <param name="rD"> Radius decrement coefficient: nR = cR * dR </param>
        /// <returns> Minimization path as array of tuples. A: point, B: function value in point. </returns>
        public (Vector point, double f)[] MinimizeFunction(Func<Vector, double> f,
                                                           Vector startPoint,
                                                           double directionGenerationAttemptBeforeRadiusDecrementCount,
                                                           double rMin, double rInit, double rD)
        {
            LinkedList<(Vector point, double f)> result = new LinkedList<(Vector point, double f)>();
            var fStartPoint = f(startPoint);

            result.AddLast((startPoint, fStartPoint));
            
            _stochasticIteration(f,
                                 result, startPoint, fStartPoint,
                                 directionGenerationAttemptBeforeRadiusDecrementCount,
                                 rMin, rInit, rD);

            return result.ToArray();
        }

        private void _stochasticIteration(Func<Vector, double> f,
                                          LinkedList<(Vector point, double f)> acc, Vector p0, double fP0,
                                          double directionGenerationAttemptBeforeRadiusDecrementCount,
                                          double rMin, double r, double rD)
        {
            if (r <= rMin) 
            {
                return;
            }

            for (var i = 0; i < directionGenerationAttemptBeforeRadiusDecrementCount; i++)
            {
                var direction = _generateDirection();
                var g = D2ToD1LineFunctionConverter.Convert(f, p0, direction);

                var l = _searcher1D.Minimize1dFunction(g, r);
                Vector p = (Vector)(p0 + DenseVector.OfArray(new double[] { l * Math.Cos(direction), l * Math.Sin(direction) }));

                var fP = f(p);

                if (fP < fP0)
                {
                    acc.AddLast((p, fP));
                    _stochasticIteration(f,
                                         acc, p, fP,
                                         directionGenerationAttemptBeforeRadiusDecrementCount,
                                         rMin, r, rD);
                    return;
                }
            }

            _stochasticIteration(f,
                                 acc, p0, fP0,
                                 directionGenerationAttemptBeforeRadiusDecrementCount,
                                 rMin, r * rD, rD);
        }

        private double _generateDirection()
        {
            return _random.NextDouble() * Math.PI * 2.0;
        }
    }
}

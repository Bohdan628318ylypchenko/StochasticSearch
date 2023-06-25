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
        /// <param name="directionGenerationAttemptBeforeRadiusDecrementCount"> 
        ///     Count of direction generation attempts to try finding min before lowering radius.
        /// </param>
        /// <param name="rMin"> Minimum search radius. </param>
        /// <param name="rInit"> Initial search radius. </param>
        /// <param name="rD"> Radius decrement coefficient: nR = cR * dR </param>
        /// <returns> Minimization path as array of tuples. A: point, B: function value in point. </returns>
        public Vector[] MinimizeFunction(Func<Vector, double> f,
                                         Vector startPoint,
                                         double directionGenerationAttemptBeforeRadiusDecrementCount,
                                         double rMin, double rInit, double rD)
        {
            LinkedList<Vector> result = new();
            result.AddLast(startPoint);
            var fStartPoint = f(startPoint);
            
            StochasticIteration(f,
                                startPoint, fStartPoint,
                                directionGenerationAttemptBeforeRadiusDecrementCount,
                                rMin, rInit, rD,
                                result);

            return result.ToArray();
        }

        /// <summary>
        /// Minimizes given function by Stochastic search in given bounds.
        /// ff = f(x,y) + pD() * p(calc_d(x,y)) 
        /// </summary>
        /// <param name="f"> Function to minimize. </param>
        /// <param name="bounds"> Bounds to search minimum in. At least 3 elements required. </param>
        /// <param name="penalty"> Function - bounds penalty. </param>
        /// <param name="penaltyCoefficients"> Array of penalty coefficients. </param>
        /// <param name="directionGenerationAttemptBeforeRadiusDecrementCount"> 
        ///     Count of direction generation attempts to try finding min before lowering radius.
        /// </param>
        /// <param name="rMin"> Minimum search radius. </param>
        /// <param name="rInit"> Initial search radius. </param>
        /// <param name="rD"> Radius decrement coefficient: nR = cR * dR </param>
        /// <returns> Minimization path as array of tuples. A: point, B: function value in point. </returns>
        public Vector[] MinimizeFunctionInBounds(Func<Vector, double> f,
                                                 Vector[] bounds,
                                                 Func<double, double> penalty, double[] penaltyCoefficients,
                                                 Vector startPoint,
                                                 double directionGenerationAttemptBeforeRadiusDecrementCount,
                                                 double rMin, double rInit, double rD)
        {
            LinkedList<Vector> result = new();
            result.AddLast(startPoint);

            foreach (var currentPenaltyCoefficient in penaltyCoefficients)
            {
                Func<Vector, double> ff;
                if (IsPointInBounds(result.Last.Value, bounds))
                {
                    ff = f;
                }
                else
                {
                    ff = v => f(v) + currentPenaltyCoefficient * penalty(DistanceToBounds(v, bounds));
                }

                StochasticIteration(ff,
                                    result.Last.Value, ff(result.Last.Value),
                                    directionGenerationAttemptBeforeRadiusDecrementCount,
                                    rMin, rInit, rD,
                                    result);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Runs recursive Stochastic iteration.
        /// </summary>
        /// <param name="f"> Function to minimize. </param>
        /// <param name="v0"> Current point. </param>
        /// <param name="fv0"> f(p0) </param>
        /// <param name="directionGenerationAttemptBeforeRadiusDecrementCount"> 
        ///     Count of direction generation attempts to try finding min before lowering radius.
        /// </param>
        /// <param name="rMin"> Minimum search radius. </param>
        /// <param name="r"> Current search radius. </param>
        /// <param name="rD"> Radius decrement coefficient: nR = cR * dR </param>
        /// <param name="acc"> Linked list to save steps into. </param>
        private void StochasticIteration(Func<Vector, double> f,
                                         Vector v0, double fv0,
                                         double directionGenerationAttemptBeforeRadiusDecrementCount,
                                         double rMin, double r, double rD,
                                         LinkedList<Vector> acc)
        {
            if (r <= rMin) 
            {
                return;
            }

            for (var i = 0; i < directionGenerationAttemptBeforeRadiusDecrementCount; i++)
            {
                var direction = GenerateDirection();
                var g = ConvertD2FuncToD1LineFunc(f, v0, direction);

                var l = _searcher1D.Minimize1dFunction(g, r);
                Vector v = VectorStepVectorSum(v0, l, direction);

                var fv = f(v);

                if (fv < fv0)
                {
                    acc.AddLast(v);
                    StochasticIteration(f,
                                        v, fv,
                                        directionGenerationAttemptBeforeRadiusDecrementCount,
                                        rMin, r, rD,
                                        acc);
                    return;
                }
            }

            StochasticIteration(f,
                                v0, fv0,
                                directionGenerationAttemptBeforeRadiusDecrementCount,
                                rMin, r * rD, rD,
                                acc);
        }

        /// <summary>
        /// Checks if point is in bounds.
        /// </summary>
        /// <param name="v"> Point - candidate. </param>
        /// <param name="bounds"> Bounds as array of pikes. </param>
        /// <returns> true if in bounds, else false. </returns>
        private bool IsPointInBounds(Vector v, Vector[] bounds)
        {
            bool result = false;
            int j = bounds.Length - 1;
            for (int i = 0; i < bounds.Length; i++)
            {
                if (bounds[i][1] < v[1] && bounds[j][1] >= v[1] || 
                    bounds[j][1] < v[1] && bounds[i][1] >= v[1])
                {
                    if (bounds[i][0] + (v[1] - bounds[i][1]) /
                       (bounds[j][1] - bounds[i][1]) *
                       (bounds[j][0] - bounds[i][0]) < v[0])
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        /// <summary>
        /// Calculates distance from point to polygon, represented by bounds.
        /// </summary>
        /// <param name="v"> Point. </param>
        /// <param name="bounds"> Polygon as array of pikes. </param>
        /// <returns> Distance from point to polygon. </returns>
        private double DistanceToBounds(Vector v, Vector[] bounds)
        {
            double[] distances = new double[bounds.Length];
            for (var i = 0; i < bounds.Length; i++)
            {
                var currentSegment = bounds[(i + 1) % bounds.Length] - bounds[i];
                var currentSegmentLength = currentSegment.L2Norm();
                var v1 = v - bounds[i];
                var v2 = v - bounds[(i + 1) % bounds.Length];
                var r = currentSegment.DotProduct(v - bounds[i]);
                r /= Math.Pow(currentSegmentLength, 2);
                if (r < 0)
                {
                    distances[i] = v1.L2Norm();
                }
                else if (r > 1)
                {
                    distances[i] = v2.L2Norm();
                }
                else
                {
                    distances[i] = Math.Sqrt(Math.Pow(v1.L2Norm(), 2) - Math.Pow(r * currentSegmentLength, 2));
                }
            }

            return distances.Min();
        }

        /// <summary>
        /// Generates random direction as angle [0; 2PI]
        /// </summary>
        private double GenerateDirection()
        {
            return _random.NextDouble() * Math.PI * 2.0;
        }

        /// <summary>
        /// Converts given 2d function into 1d line-directed function.
        /// </summary>
        /// <param name="f"> Original 2d function. </param>
        /// <param name="v0"> Point to pass line though. </param>
        /// <param name="a"> Line angle. </param>
        /// <returns> 1d line-directed function.  </returns>
        private Func<double, double> ConvertD2FuncToD1LineFunc(Func<Vector, double> f,
                                                               Vector v0, double a)
        {
            return r => f(VectorStepVectorSum(v0, r, a));
        }

        /// <summary>
        /// Calculates new vector as: p + r * (cos(a), sin(a))
        /// </summary>
        /// <param name="v"> Vector to add step to. </param>
        /// <param name="r"> Step vector length. </param>
        /// <param name="a"> Direction as angle from [0, 2PI]. </param>
        /// <returns></returns>
        private Vector VectorStepVectorSum(Vector v, double r, double a)
        {
            return (Vector)(v + DenseVector.OfArray(new double[] { r * Math.Cos(a), r * Math.Sin(a) }));
        }
    }
}

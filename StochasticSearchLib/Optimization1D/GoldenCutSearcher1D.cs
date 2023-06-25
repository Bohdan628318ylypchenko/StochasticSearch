namespace StochasticSearchLib.Optimization1D
{
    /// <summary>
    /// Golden cut 1d minimum search.
    /// </summary>
    public sealed class GoldenCutSearcher1D : ISearcher1D
    {
        private readonly double _goldenEpsilon;
        private readonly double _initialSvenDelta;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="goldernEpsilon"> Minimum interval length. </param>
        public GoldenCutSearcher1D(double goldernEpsilon, double initialSvenDelta)
        {
            _goldenEpsilon = goldernEpsilon;
            _initialSvenDelta = initialSvenDelta;
        }

        /// <summary>
        /// Contract implementation.
        /// </summary>
        /// <param name="g"> Function to minimize. </param>
        /// <param name="r"> Area radius to search min in. </param>
        /// <returns> function minimum as double. </returns>
        public double Minimize1dFunction(Func<double, double> g, double r)
        {
            var (a, b) = SvenInterval(g, r);
            return GoldenCut(g, a, b);
        }

        /// <summary>
        /// Golden cut implementation (with recursion).
        /// </summary>
        /// <param name="g"> Function to minimize. </param>
        /// <param name="a"> Interval left bound. </param>
        /// <param name="b"> Interval right bound. </param>
        /// <returns></returns>
        private double GoldenCut(Func<double, double> g,
                                 double a, double b)
        {
            double l = b - a;

            if (l <= _goldenEpsilon) { return Math.Min(a, b); }

            double x1 = a + 0.382 * l;
            double x2 = a + 0.618 * l;

            return g(x1) > g(x2) ?
            GoldenCut(g, x1, b) : GoldenCut(g, a, x2);
        }

        /// <summary>
        /// Calculates Sven interval for 1d function.
        /// </summary>
        /// <param name="f"> 1D function to calculate for. </param>
        /// <param name="x0"> Start point. </param>
        /// <returns> Sven interval. </returns>
        private (double a, double b) SvenInterval(Func<double, double> f,
                                                  double x0)
        {
            // Selecting direction and calculating new delta
            var (direction, updatedDelta) = SelectDirectionUpdateDelta(f, x0, _initialSvenDelta);

            // Zero direction check
            if (direction == 0)
            {
                // Zero direction -> x0 - delta, x0 + delta contains min
                return (x0 - _initialSvenDelta, x0 + _initialSvenDelta);
            }
            else
            {
                // Iterating in selected direction
                return SvenIterate(f, x0, direction * updatedDelta);
            }
        }

        /// <summary>
        /// Selects direction to move at.
        /// Calculates new delta value.
        /// </summary>
        /// <param name="f"> Function to minimize. </param>
        /// <param name="x0"> Start point. </param>
        /// <param name="d"> Delta of step. </param>
        /// <returns> Selected direction and (possibly) updated delta. </returns>
        private (int direction, double updatedDelta) SelectDirectionUpdateDelta(Func<double, double> f,
                                                                                double x0, double d)
        {
            // Interval
            var points = new double[3] { x0 - d, x0, x0 + d };

            // Bound - middle function values
            var fpoints = points.Select(p => f(p)).ToArray();
            if (fpoints[0] >= fpoints[1] && fpoints[1] >= fpoints[2])
            {
                // Move right, no delta correction
                return (1, d);
            }
            else if (fpoints[0] <= fpoints[1] && fpoints[1] <= fpoints[2])
            {
                // Move left, no delta correction
                return (-1, d);
            }
            else if (fpoints[0] >= fpoints[1] && fpoints[1] <= fpoints[2])
            {
                // Don't move
                return (0, d);
            }
            else
            {
                // Function is multi-modal on interval, try smaller interval.
                return SelectDirectionUpdateDelta(f, x0, d / 2);
            }
        }

        /// <summary>
        /// Iterates according to Sven algorithm.
        /// </summary>
        /// <param name="f"> Function to search interval for. </param>
        /// <param name="cborder"> Current bound. </param>
        /// <param name="step"> Current step. </param>
        /// <returns> Tuple - final interval. </returns>
        private (double, double) SvenIterate(Func<double, double> f,
                                             double cborder, double step)
        {
            // Calculating new border
            double nborder = cborder + step;

            // Is value in new border bigger than current border value?
            if (f(cborder) <= f(nborder))
            {
                // Yes -> go back and return
                cborder -= (step / 2);
                return (Math.Min(cborder, nborder), Math.Max(cborder, nborder));
            }
            else
            {
                // No -> go further
                return SvenIterate(f, nborder, step * 2);
            }
        }

    }
}

namespace StochasticSearchLib.Optimization1D
{
    /// <summary>
    /// Naive 1d search.
    /// </summary>
    public sealed class NaiveSearcher1D : ISearcher1D
    {
        /// <summary>
        /// Contract implementation.
        /// </summary>
        /// <param name="g"> Function to minimize. </param>
        /// <param name="r"> Area radius to search min in. </param>
        /// <returns> r as function minimum </returns>
        public double Minimize1dFunction(Func<double, double> g, double r)
        {
            return r;
        }
    }
}

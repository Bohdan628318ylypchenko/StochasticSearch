namespace StochasticSearchLib.Optimization1D
{
    /// <summary>
    /// Defines 1d searcher contract.
    /// </summary>
    public interface ISearcher1D
    {
        /// <summary>
        /// Returns minimum of given 1d function.
        /// </summary>
        /// <param name="g"> Function to minimize. </param>
        /// <param name="r"> Area radius to search min in. </param>
        /// <returns> Function minimum as double. </returns>
        public abstract double Minimize1dFunction(Func<double, double> g, double r);
    }
}

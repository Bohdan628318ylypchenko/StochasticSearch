using MathNet.Numerics.LinearAlgebra.Double;

namespace StochasticSearchLib
{
    /// <summary>
    /// 2d -> 1d l-d
    /// </summary>
    internal static class D2ToD1LineFunctionConverter
    {
        /// <summary>
        /// Converts given 2d function into 1d line-directed function.
        /// </summary>
        /// <param name="f"> Original 2d function. </param>
        /// <param name="p0"> Point to pass line though. </param>
        /// <param name="a"> Line angle. </param>
        /// <returns> 1d line-directed function.  </returns>
        public static Func<double, double> Convert(Func<Vector, double> f,
                                                   Vector p0, double a)
        {
            return r =>
            f.Invoke(DenseVector.OfArray(new double[] { p0[0] + r * Math.Cos(a),
                                                        p0[1] + r * Math.Sin(a) }));
        }
    }
}

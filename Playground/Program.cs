using MathNet.Numerics.LinearAlgebra.Double;
using StochasticSearchLib;
using StochasticSearchLib.Optimization1D;

namespace Playground
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Func<Vector, double> f = x => Math.Pow(1 - x[0], 2) + 100 * Math.Pow(x[1] - x[0] * x[0], 2);
            Func<Vector, double> f = x => Math.Pow(x[0] - 3, 2) + Math.Pow(x[1] + 4, 2);
            Vector x0 = DenseVector.OfArray(new double[2] { -1, 1 });

            ISearcher1D searcher1D = new NaiveSearcher1D();

            //ISearcher1D searcher1D = new GoldenCutSearcher1D(0.008, 0.1);

            StochasticSearcher stochasticSearcher = new StochasticSearcher(searcher1D, 450);

            Vector[] bounds = new Vector[4];
            bounds[0] = DenseVector.OfArray(new double[2] { -2, 2 });
            bounds[1] = DenseVector.OfArray(new double[2] { -2, -1 });
            bounds[2] = DenseVector.OfArray(new double[2] { 1, -1 });
            bounds[3] = DenseVector.OfArray(new double[2] { 1, 1 });

            double[] penaltyCoefficients = Enumerable.Range(1, 100).Select(i => i * 0.01).ToArray();
            Func<double, double> penalty = x => 100 * x * x;

            //var result = stochasticSearcher.MinimizeFunction(f, x0, 100, 0.001, 0.1, 0.9);

            var result = stochasticSearcher.MinimizeFunctionInBounds(f,
                                                                     bounds, penalty, penaltyCoefficients,
                                                                     x0, 100, 0.001, 0.5, 0.9);

            double[] X = result.Select(v => v[0]).ToArray();
            double[] Y = result.Select(v => v[1]).ToArray();

            Console.WriteLine(result.Length);
            for (var i = 0; i < result.Length; i++)
            {
                Console.Write("{0:F7} ; {1:F7}\n", X[i], Y[i]);
            }

            var plt = new ScottPlot.Plot(1920, 1080);
            plt.AddScatter(X, Y);
            plt.SaveFig("Result.png");
        }
    }
}
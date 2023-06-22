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
            Vector x0 = DenseVector.OfArray(new double[2] { -1.2, 0 });

            ISearcher1D searcher1D = new NaiveSearcher1D();

            //ISearcher1D searcher1D = new GoldenCutSearcher1D(0.008, 0.1);

            StochasticSearcher stochasticSearcher = new StochasticSearcher(searcher1D, 450);

            var result = stochasticSearcher.MinimizeFunction(f, x0, 100, 0.001, 0.1, 0.9);

            double[] X = result.Select(v => v.point[0]).ToArray();
            double[] Y = result.Select(v => v.point[1]).ToArray();

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
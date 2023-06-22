using StochasticSearchLib.Optimization1D;

namespace Tests
{
    [TestClass]
    public class GoldenCut
    {
        [TestMethod]
        public void GoldenCutTest1()
        {
            Func<double, double> g = x => 5 * x * x - 200 * x - 43;

            GoldenCutSearcher1D searcher1D = new GoldenCutSearcher1D(0.01, 5);
            var min = searcher1D.Minimize1dFunction(g, 10);

            Assert.IsTrue(Math.Abs(min - (20)) < 0.01);
        }

        [TestMethod]
        public void InstantMinTest()
        {
            // Function to minimize
            Func<double, double> g = x => x * x;

            // Minimum search
            GoldenCutSearcher1D searcher1D = new GoldenCutSearcher1D(0.0001, 0.1);
            var min = searcher1D.Minimize1dFunction(g, 0.0001);

            // Asserting
            Assert.IsTrue(Math.Abs(min) < 0.0001);
        }
    }
}

using StochasticSearchLib.Optimization1D;

namespace Tests
{
    [TestClass]
    public class NaiveSearcher
    {
        [TestMethod]
        public void NaiveSearchTest()
        {
            Func<double, double> g = x => 5 * x * x - 200 * x - 43;

            NaiveSearcher1D searcher1D = new NaiveSearcher1D();
            var min = searcher1D.Minimize1dFunction(g, 10);

            Assert.AreEqual(10, min);
        }
    }
}

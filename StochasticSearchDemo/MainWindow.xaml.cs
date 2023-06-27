using System;
using System.Linq;
using System.Windows;
using MathNet.Numerics.LinearAlgebra.Double;
using StochasticSearchLib;
using StochasticSearchLib.Optimization1D;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Windows.Input;

namespace StochasticSearchDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isGoldenCut = false;
        private bool _isBounded = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private Func<double, double, double> CreateFunctionXYFromString(string fSrc)
        {
            var str = "(x, y) => " + fSrc;
            var options = ScriptOptions.Default.AddImports(new string[] { "System" });
            return CSharpScript.EvaluateAsync<Func<double, double, double>>(str, options).Result;
        }

        private Func<T, double> CreateFunctionXFromString<T>(string fSrc)
        {
            var str = "(x) => " + fSrc;
            var options = ScriptOptions.Default.AddImports(new string[] { "System" });
            return CSharpScript.EvaluateAsync<Func<T, double>>(str, options).Result;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait; 

            Plot.Plot.Clear();

            Func<MathNet.Numerics.LinearAlgebra.Double.Vector, double> f = null;
            MathNet.Numerics.LinearAlgebra.Double.Vector x0;
            ISearcher1D searcher1D;
            double goldenCutEpsilon, goldenCutSvenDelta;
            int seed;
            int directionGenerationAttemptBeforeRadiusDecrementCount;
            double rMin, rInit, rD;
            Func<double, double> penalty;
            int penaltyCoefCount;
            Func<int, double> penaltyCoefFunction;
            double[] penaltyCoefficients;
            MathNet.Numerics.LinearAlgebra.Double.Vector[] bounds = null;
            StochasticSearcher stochasticSearcher;
            MathNet.Numerics.LinearAlgebra.Double.Vector[] result;
            try
            {
                var tf = CreateFunctionXYFromString(FunctionTextBox.Text);
                f = v => tf(v[0], v[1]);    
                var x0s = StartPointTextBox.Text.Split(' ');
                x0 = DenseVector.OfArray(new double[] { Double.Parse(x0s[0]),
                                                        Double.Parse(x0s[1] )});
                if (_isGoldenCut)
                {
                    goldenCutEpsilon = Double.Parse(GCEpsilonTextBox.Text);
                    goldenCutSvenDelta = Double.Parse(GCSvenDeltaTextBox.Text);
                    searcher1D = new GoldenCutSearcher1D(goldenCutEpsilon, goldenCutSvenDelta);
                }
                else
                {
                    searcher1D = new NaiveSearcher1D();
                }

                seed = Int32.Parse(SeedTextBox.Text);
                directionGenerationAttemptBeforeRadiusDecrementCount = Int32.Parse(DGACountTextBox.Text);
                rMin = double.Parse(RMinTextBox.Text);
                rInit = double.Parse(RInitTextBox.Text);
                rD = double.Parse(RdTextBox.Text);

                stochasticSearcher = new StochasticSearcher(searcher1D, seed);

                if (_isBounded)
                {
                    penalty = CreateFunctionXFromString<double>(PenaltyTextBox.Text);
                    penaltyCoefCount = Int32.Parse(PenaltyCoefCountTextBox.Text);
                    penaltyCoefFunction = CreateFunctionXFromString<Int32>(PenaltyCoefFunctionTextBox.Text);
                    penaltyCoefficients = Enumerable.Range(1, penaltyCoefCount)
                                                    .Select(penaltyCoefFunction)
                                                    .ToArray();
                    var lines = BoundsTextBox.Text.Split('\n');
                    bounds = new MathNet.Numerics.LinearAlgebra.Double.Vector[lines.Length];
                    for (var i = 0; i < lines.Length; i++)
                    {
                        var coords = lines[i].Split(' ');
                        var x = Double.Parse(coords[0]);
                        var y = Double.Parse(coords[1]);
                        bounds[i] = DenseVector.OfArray(new double[] { x, y });
                    }
                    result = stochasticSearcher.MinimizeFunctionInBounds(f,
                                                                         bounds, penalty, penaltyCoefficients,
                                                                         x0, directionGenerationAttemptBeforeRadiusDecrementCount,
                                                                         rMin, rInit, rD);
                    double[] bX = bounds.Select(v => v[0]).ToArray();
                    double[] bY = bounds.Select(v => v[1]).ToArray();
                    Plot.Plot.AddPolygon(bX, bY, Plot.Plot.GetNextColor(.7));
                }
                else
                {
                    result = stochasticSearcher.MinimizeFunction(f,
                                                                 x0,
                                                                 directionGenerationAttemptBeforeRadiusDecrementCount,
                                                                 rMin, rInit, rD);
                }
                double[] rX = result.Select(v => v[0]).ToArray();
                double[] rY = result.Select(v => v[1]).ToArray();

                ResultTextBox.Text = "";
                ResultTextBox.Text += "Function: " + FunctionTextBox.Text + "\n";
                ResultTextBox.Text += "Start point: " + StartPointTextBox.Text + "\n";
                if (_isGoldenCut)
                {
                    ResultTextBox.Text += "1D: golden cut" + "\n";
                    ResultTextBox.Text += "    Epsilon: " + GCEpsilonTextBox.Text + "\n";
                    ResultTextBox.Text += "    Sven Delta: " + GCSvenDeltaTextBox.Text + "\n";
                }
                else
                {
                    ResultTextBox.Text += "1D: naive" + "\n";
                }
                ResultTextBox.Text += "Seed: " + SeedTextBox.Text + "\n";
                ResultTextBox.Text += "Direction generation attempt before radius decrement count: " + DGACountTextBox.Text + "\n";
                ResultTextBox.Text += "Minimum radius: " + RMinTextBox.Text + "\n";
                ResultTextBox.Text += "Initial radius: " + RInitTextBox.Text + "\n";
                ResultTextBox.Text += "Radius decrement: " + RdTextBox.Text + "\n";
                if (_isBounded)
                {
                    ResultTextBox.Text += "Bounds: true" + "\n";
                    ResultTextBox.Text += "Penalty function: " + PenaltyTextBox.Text + "\n";
                    ResultTextBox.Text += "Penalty coef count: " + PenaltyCoefCountTextBox.Text + "\n";
                    ResultTextBox.Text += "Penalty coef function: " + PenaltyCoefFunctionTextBox.Text + "\n";
                    foreach (var b in bounds)
                    {
                        ResultTextBox.Text += b[0] + " " + b[1] + "\n";
                    }
                }
                ResultTextBox.Text += "\n=========================================\n";
                for (var i = 0; i < result.Length; i++)
                {
                    ResultTextBox.Text += rX[i] + " " + rY[i] + '\n';
                }
                ResultTextBox.Text += "Step count: " + result.Length;

                Plot.Plot.AddScatter(rX, rY);

                Plot.Plot.AddHorizontalLine(0);
                Plot.Plot.AddVerticalLine(0);

                Plot.Refresh();
            }
            catch
            {
                MessageBox.Show("Invalid input!", "Oops!");
            }

            Mouse.OverrideCursor = null;
        }

        private void GoldenCutRadioButton_Click(object sender, RoutedEventArgs e)
        {
            _isGoldenCut = !_isGoldenCut;
            GCEpsilonTextBox.IsEnabled = _isGoldenCut;
            GCSvenDeltaTextBox.IsEnabled = _isGoldenCut;
            GoldenCutRadioButton.IsChecked = _isGoldenCut;
        }

        private void BoundsRadioButton_Click(object sender, RoutedEventArgs e)
        {
            _isBounded = !_isBounded;
            PenaltyTextBox.IsEnabled = _isBounded;
            PenaltyCoefCountTextBox.IsEnabled = _isBounded;
            PenaltyCoefFunctionTextBox.IsEnabled = _isBounded;
            BoundsTextBox.IsEnabled = _isBounded;
            BoundsRadioButton.IsChecked = _isBounded;
        }
    }
}

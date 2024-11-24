/*
This is a program that excecutes three types of optimization algorithms
    tracks the number of cycles and goal (objective) function calls so that
    the algorithms' performance could be compared with eachother.
The algorithms are:
    - Gradient Descent
    - Fastest Descent, that uses golden section search to find the optimal step (lambda) size
    - Deformed Simplex - Nelder-Mead algorithm

Notes:
    - this is a study laboratory work, so the user interface is minimal.
    - this the results are printed to the console.
    - the 3D and simplex plots are displayed in a separate window.
    - to run the algorithms, the appropraite lines in main must be uncommented.
    - the objective function is defined due to the specific task. IT CAN BE CHANGED
        TO ANY OTHER FUNCTION, however, the partial gradients must be adjusted accordingly.
    - the gradient descent and fastest descent algorithms are implemented in
        the Program class, while the Simplex is defined in the sepereate Simplex class.
 */

using Plotly.NET;

namespace OptimizavimasBeApribojimu
{
    class Program
    {
        static void Main(String[] args)
        {
            // Define the coordinates of a starting point.
            double x = 0;
            double y = 0;
            x = 1;
            y = 1;

            double epsilon = 1e-5; // variable that defines end of algorithms.
            Program program = new Program();


            //(List<(double, double, double)> pointTimeLine, int functionCallsCount, int cycles) = program.gradientDescent(x, y, epsilon);
            (List<(double, double, double)> pointTimeLine, int functionCallsCount, int cycles) = program.FastestDescent(x, y, epsilon);

            foreach (var point in pointTimeLine)
            {
                Console.WriteLine($"X: {point.Item1}, Y: {point.Item2}, Žingsnio ilgis: {point.Item3}");
            }
            program.Display3D(pointTimeLine);

            //Simplex simplex_ = new Simplex();
            //(List<double[][]> simplexTimeLine, int functionCallsCount, int cycles) = simplex_.Run(x, y, epsilon);
            //program.DisplaySimplexTimeline(simplexTimeLine);

            Console.WriteLine($"\nTikslo funkcija kviesta {functionCallsCount} kartų, per {cycles} ciklus.");
        }

        private (List<(double, double, double)>, int, int) FastestDescent(double x, double y, double epsilon)
        {
            List<(double, double, double)> pointTimeLine = new();
            pointTimeLine.Add((x, y, 1));
            int cycles = 0;
            int targetFunctionCalled = 0;
            double t = (-1 + Math.Sqrt(5)) / 2;
            double leftBorder = 0;
            double rightBorder = 5;

            while (cycles < 5000)
            {
                (double gradX, double gradY) = PartialGradients(x, y);
                targetFunctionCalled += 2;
                if (Math.Abs(gradX) < epsilon && Math.Abs(gradY) < epsilon)
                {
                    Console.WriteLine($"Metodas pasibaigė, nes gradientų reikšmės mažesnės" +
                        $" už epsilon {epsilon}: gradX: {gradX}, gradY: {gradY}\n");
                    break;
                }

                double interval = rightBorder - leftBorder;
                double lambda1 = rightBorder - t * interval; 
                double lambda2 = leftBorder + t * interval;

                double f1 = ObjectiveFunction(x - lambda1 * gradX, y - lambda1 * gradY);
                double f2 = ObjectiveFunction(x - lambda2 * gradX, y - lambda2 * gradY);
                targetFunctionCalled += 2;

                while (interval > epsilon)
                {
                    if (f2 < f1)
                    {
                        leftBorder = lambda1;
                        lambda1 = lambda2;
                        f1 = f2;
                        interval = rightBorder - leftBorder;
                        lambda2 = leftBorder + t * interval;
                        f2 = ObjectiveFunction(x - lambda2 * gradX, y - lambda2 * gradY);
                        targetFunctionCalled++;
                    }
                    else if (f1 < f2)
                    {
                        rightBorder = lambda2;
                        lambda2 = lambda1;
                        f2 = f1;
                        interval = rightBorder - leftBorder;
                        lambda1 = rightBorder - t * interval;
                        f1 = ObjectiveFunction(x - lambda1 * gradX, y - lambda1 * gradY);
                        targetFunctionCalled++;
                    }
                    else
                    {
                        leftBorder = lambda1;
                        rightBorder = lambda2;
                        interval = rightBorder - leftBorder;
                        lambda1 = rightBorder - t * interval;
                        lambda2 = leftBorder + t * interval;
                        f1 = ObjectiveFunction(x - lambda1 * gradX, y - lambda1 * gradY);
                        f2 = ObjectiveFunction(x - lambda2 * gradX, y - lambda2 * gradY);
                        targetFunctionCalled += 2;
                    }
                    cycles++;
                }
                double lambda = (f1 < f2) ? lambda1 : lambda2;
                x -= lambda * gradX;
                y -= lambda * gradY;
                pointTimeLine.Add((x, y, lambda));
                cycles++;
            }

            return (pointTimeLine, targetFunctionCalled, cycles);
        }

        private (List<(double, double, double)>, int, int) gradientDescent(double x, double y, double epsilon)
        {
            List<(double, double, double)> pointTimeLine = new List<(double, double, double)>();
            int i = 0; pointTimeLine.Add((x, y, 1));
            int targetFunctionCalled = 0;
            while (i < 10000)
            {
                (double gradX, double gradY) = PartialGradients(x, y);
                targetFunctionCalled += 2;
                if (Math.Abs(gradX) < epsilon && Math.Abs(gradY) < epsilon)
                {
                    Console.WriteLine($"Metodas pasibaigė, nes gradientų reikšmės mažesnės" +
                        $"už epsilon {epsilon}: gradX: {gradX}, gradY: {gradY}\n");
                    break;
                }
                x -= gradX;
                y -= gradY;
                pointTimeLine.Add((x, y, 1));
                i++;
            }
            return (pointTimeLine, targetFunctionCalled, i);
        }

        private double ObjectiveFunction(double x, double y)
        {
            return -x * y * (1 - x - y) / 8;
        }
        private (double, double) PartialGradients(double x, double y)
        {
            double dx = y * (2 * x + y - 1) / 8;
            double dy = x * (2 * y + x - 1) / 8;
            return (dx, dy);
        }


        // Displaying the visualization of the optimization path for Gradient descent and Fastest Descent
        // the Simplex visualization will not work here.
        private void Display3D(List<(double x, double y, double lambda)> pointTimeLine)
        {
            var trace = Chart3D.Chart.Scatter3D<double, double, double, string>(
                pointTimeLine.Select(p => p.x).ToArray(),
                pointTimeLine.Select(p => p.y).ToArray(),
                pointTimeLine.Select(p => ObjectiveFunction(p.x, p.y)).ToArray(),
                mode: StyleParam.Mode.Lines_Markers,
                Name: "Optimization Descent Path"
            )
            .WithMarkerStyle(
                Size: 6,
                Color: Color.fromString("red"),
                Symbol: StyleParam.MarkerSymbol.Star
            )
            .WithLineStyle(
                Color: Color.fromString("blue"),
                Dash: StyleParam.DrawingStyle.Solid
            );


            int gridSize = 80;
            var xRange = Enumerable.Range(0, gridSize).Select(i => i * 1.0 / gridSize).ToArray();
            var yRange = Enumerable.Range(0, gridSize).Select(i => i * 1.0 / gridSize).ToArray();

            //var zSurface = new double[gridSize][];
            //for (int i = 0; i < gridSize; i++)
            //{
            //    zSurface[i] = new double[gridSize];
            //    for (int j = 0; j < gridSize; j++)
            //    {
            //        zSurface[i][j] = ObjectiveFunction(xRange[i], yRange[j]);
            //    }
            //}

            //var zSurface = new List<double>();
            //foreach (var x in xRange)
            //{
            //    foreach (var y in yRange)
            //    {
            //        zSurface.Add(ObjectiveFunction(x, y));
            //    }
            //}

            //var surfaceTrace = Chart3D.Chart.Surface<double, double, double, string, string>(
            //    xRange,
            //    yRange,
            //    zSurface
            //)
            //.WithSurfaceStyle(
            //    ColorScale: StyleParam.Colorscale.Viridis
            //);

            //var chart = Chart.Combine(new[] { surfaceTrace, trace })
            var chart = trace
                .WithXAxisStyle(Title.init("x"))
                .WithYAxisStyle(Title.init("y"))
                .WithZAxisStyle(Title.init("z"))
                .WithSize(1000, 1000)
                .WithGeoStyle();
            chart.Show();
        }


        // Displaying the visualization of the optimization path specifically for Simplex
        private void DisplaySimplexTimeline(List<double[][]> simplexTimeLine)
        {
            var traces = new List<GenericChart>();
            int i = 0;
            foreach (var simplex in simplexTimeLine)
            {
                i++;
                List<double> xValues = simplex.Select(p => p[0]).ToList();
                List<double> yValues = simplex.Select(p => p[1]).ToList();
                xValues.Add(xValues[0]);
                yValues.Add(yValues[0]);


                Console.WriteLine($"{xValues[0]} & {yValues[0]} & {xValues[1]} & {yValues[1]} & {xValues[2]} & {yValues[2]} \\\\");

                var trace = Chart2D.Chart.Scatter<double, double, string>(
                    xValues,
                    yValues,
                    mode: StyleParam.Mode.Lines_Markers
                )
                .WithLineStyle(
                    Color: Color.fromString("blue"),
                    Dash: StyleParam.DrawingStyle.Solid,
                    Width: 1
                )
                .WithMarkerStyle(
                    Size: 5,
                    Color: Color.fromString("red"),
                    Symbol: StyleParam.MarkerSymbol.Circle
                );

                traces.Add(trace);
            }

            var chart = Chart.Combine(traces)
                .WithXAxisStyle(Title.init("x"))
                .WithYAxisStyle(Title.init("y"))
                .WithSize(1000, 1000);

            chart.Show();
        }
    }
}

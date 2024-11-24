using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OptimizavimasBeApribojimu
{
    public class Simplex
    {
        public Simplex() { }

        public (List<double[][]>, int, int) Run(double x, double y, double epsilon)
        {
            List<double[][]> simplexTimeLine = new();
            double atspindys = 1;
            double sumazintasAtspindys = 0.5;
            double istemptasAtspindys = 2;
            double edgeSize = 1;

            double[][] simplex = GenerateSimplex(x, y, edgeSize);
            int functionCallsCount = 3;
            int cycles = 0;

            while (getLongestEdgeLength(simplex) > epsilon)
            {
                Array.Sort(simplex, (a, b) => b[2].CompareTo(a[2]));
                simplexTimeLine.Add((double[][])simplex.Clone());
                Console.WriteLine($"F-jos reikšmės: {simplex[0][2]}, {simplex[1][2]}, {simplex[2][2]}");
                Console.WriteLine($"Simplekso taškai: ({simplex[0][0]}; {simplex[0][1]}) | ({simplex[1][0]}; {simplex[1][1]}) | ({simplex[2][0]}; {simplex[2][1]}) |");
                cycles++;

                double[] best = simplex[2];
                double[] worst = simplex[0];
                double[] secondWorst = simplex[1];

                double[] centroid = CalculateCentroid(simplex, worst);
                double[] newPoint = Reflect(centroid, worst, atspindys);

                (simplex, int count, bool isNegative) = FixNegative(simplex, newPoint, centroid, -sumazintasAtspindys);
                functionCallsCount += count;
                if (isNegative)
                    continue;
                newPoint[2] = ObjectiveFunction(newPoint[0], newPoint[1]);
                functionCallsCount++;

                Console.WriteLine($"Atsipindys: {newPoint[2]}");

                if (newPoint[2] > best[2] && newPoint[2] < secondWorst[2])
                {
                    simplex[0] = newPoint;
                    Console.WriteLine($"liko toks pats: {newPoint[2]}\n");
                    continue;
                }
                else if (newPoint[2] < best[2])
                {
                    double[] extendedPoint = Reflect(centroid, worst, istemptasAtspindys);
                    extendedPoint[2] = ObjectiveFunction(extendedPoint[0], extendedPoint[1]);
                    functionCallsCount++;
                    (simplex, count, isNegative) = FixNegative(simplex, newPoint, centroid, -sumazintasAtspindys);
                    functionCallsCount += count;
                    if (isNegative)
                        continue;
                    simplex[0] = extendedPoint[2] < newPoint[2] ? extendedPoint : newPoint;
                    Console.WriteLine($"Istemptas: {simplex[0][2]}\n");
                }
                else
                {
                    double[] compresedPoint = Reflect(centroid, worst, sumazintasAtspindys);
                    compresedPoint[2] = ObjectiveFunction(compresedPoint[0], compresedPoint[1]);
                    functionCallsCount++;
                    if (compresedPoint[2] < secondWorst[2])
                    {
                        simplex[0] = compresedPoint;
                        Console.WriteLine($"Suspaustas: {simplex[0][2]}\n");
                    }
                    else
                    {
                        (simplex, int calls) = ContractSimplex(simplex, centroid, sumazintasAtspindys);
                        functionCallsCount += calls;
                    }
                }
            }
            return (simplexTimeLine, functionCallsCount, cycles);
        }

        double ObjectiveFunction(double x, double y)
        {
            return -x * y * (1 - x - y) / 8;
        }

        private (double[][], int) ContractSimplex(double[][] simplex, double[] centroid, double phi)
        {
            double[] contractedPoint = Reflect(centroid, simplex[0], -phi);
            contractedPoint[2] = ObjectiveFunction(contractedPoint[0], contractedPoint[1]);
            int functionCallsCount = 1;
            if (contractedPoint[2] < simplex[1][2])
            {
                simplex[0] = contractedPoint;
                Console.WriteLine($"Suspaustas: {simplex[0][2]}\n");
            }
            else
            {
                Console.WriteLine("Simpleksas sumazejo\n");
                simplex = ShrinkSimplex(simplex);
                functionCallsCount += 2;
            }
            return (simplex, functionCallsCount);
        }

        private (double[][], int, bool) FixNegative(double[][] simplex, double[] point, double[] centroid, double phi)
        {
            int functionCallsCount = 0;
            bool isNegative = false;
            if (point[0] < 0 || point[1] < 0)
            {
                (simplex, int calls) = ContractSimplex(simplex, centroid, phi);
                functionCallsCount += calls;
                isNegative = true;
            }
            return (simplex, functionCallsCount, isNegative);
        }

        private double[][] ShrinkSimplex(double[][] simplex)
        {
            double x = simplex[0][0];
            double y = simplex[0][1];
            double x2 = (x + simplex[1][0]) / 2;
            double y2 = (y + simplex[1][1]) / 2;
            double x3 = (x + simplex[2][0]) / 2;
            double y3 = (y + simplex[2][1]) / 2;
            double[][] newSimplex =
                [
                    new double[] { x, y, simplex[0][2] },
                    new double[] { x2, y2, ObjectiveFunction(x2, y2) },
                    new double[] { x3, y3, ObjectiveFunction(x3, y3) }
                ];
            return newSimplex;
        }

        private double[] CalculateCentroid(double[][] simplex, double[] exclude)
        {
            int length = 2; // simplex[0].Length;
            double[] centroid = new double[length + 1];
            foreach (var point in simplex)
            {
                if (point != exclude)
                {
                    for (int i = 0; i < length; i++)
                        centroid[i] += point[i];
                }
            }

            length = 2; //simplex.Length -1;
            for (int i = 0; i < length; i++)
                centroid[i] /= length;
            return centroid;
        }

        private double[] Reflect(double[] centroid, double[] worst, double phi)
        {
            int length = worst.Length;
            double[] reflected = new double[length];
            for (int i = 0; i < length - 1; i++)
                reflected[i] = worst[i] + (1 + phi) * (centroid[i] - worst[i]);
            return reflected;
        }

        private double[][] GenerateSimplex(double x, double y, double edgeSize)
        {
            double x2 = x + edgeSize;
            double y2 = y;
            double x3 = x + edgeSize / 2;
            double y3 = y + Math.Sqrt(Math.Pow(edgeSize, 2) - Math.Pow(edgeSize / 2, 2));

            double[][] simplex =
            [
                new double[] { x, y, ObjectiveFunction(x, y) },
                new double[] { x2, y2, ObjectiveFunction(x2, y2) },
                new double[] { x3, y3, ObjectiveFunction(x3, y3) },
            ];
            return simplex;
        }

        private double[][] GenerateSimplex2(double x, double y, double edgeSize)
        {
            double beta1 = (Math.Sqrt(3) + 1) / (2 * Math.Sqrt(2)) * edgeSize;
            double beta2 = (Math.Sqrt(3) - 1) / (2 * Math.Sqrt(2)) * edgeSize;
            // (√3 + 1) / 2√2 ir (√3 - 1) / 2√2

            double x2 = x + beta1;
            double y2 = y + beta2;
            double x3 = x + beta2;
            double y3 = y + beta1;

            double[][] simplex =
            {
                    new double[] { x, y, ObjectiveFunction(x, y) },
                    new double[] { x2, y2, ObjectiveFunction(x2, y2) },
                    new double[] { x3, y3, ObjectiveFunction(x3, y3) },
                };
            return simplex;
        }

        private double getLongestEdgeLength(double[][] simplex)
        {
            double x = Math.Sqrt(Math.Pow(simplex[0][0] - simplex[1][0], 2) + Math.Pow(simplex[0][1] - simplex[1][1], 2));
            double y = Math.Sqrt(Math.Pow(simplex[0][0] - simplex[2][0], 2) + Math.Pow(simplex[0][1] - simplex[2][1], 2));
            double z = Math.Sqrt(Math.Pow(simplex[1][0] - simplex[2][0], 2) + Math.Pow(simplex[1][1] - simplex[2][1], 2));

            return Math.Max(x, Math.Max(y, z));
        }
    }
}
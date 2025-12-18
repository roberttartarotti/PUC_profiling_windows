using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;

namespace AlgorithmBenchmark
{
    [MemoryDiagnoser]
    [RankColumn]
    public class HasDecimalsBenchmark
    {
        private double[] _testValues;

        [GlobalSetup]
        public void Setup()
        {
            var random = new Random(42);
            _testValues = new double[1000];
            
            for (int i = 0; i < _testValues.Length; i++)
            {
                // Mix de valores com e sem decimais
                _testValues[i] = random.NextDouble() * 1000;
            }
        }

        [Benchmark(Baseline = true)]
        public int Modulo()
        {
            int count = 0;
            foreach (var value in _testValues)
            {
                if (value % 1 != 0)
                    count++;
            }
            return count;
        }

        [Benchmark]
        public int MathFloor()
        {
            int count = 0;
            foreach (var value in _testValues)
            {
                if (value != Math.Floor(value))
                    count++;
            }
            return count;
        }

        [Benchmark]
        public int MathTruncate()
        {
            int count = 0;
            foreach (var value in _testValues)
            {
                if (value != Math.Truncate(value))
                    count++;
            }
            return count;
        }

        [Benchmark]
        public int CastToLong()
        {
            int count = 0;
            foreach (var value in _testValues)
            {
                if (value != (long)value)
                    count++;
            }
            return count;
        }

        [Benchmark]
        public int BitManipulation()
        {
            int count = 0;
            foreach (var value in _testValues)
            {
                long bits = BitConverter.DoubleToInt64Bits(value);
                long exponent = (bits >> 52) & 0x7FF;
                long mantissa = bits & 0xFFFFFFFFFFFFF;
                
                // Se exponent < 1023, tem decimais
                // Se mantissa != 0 após shift apropriado, tem decimais
                if (exponent < 1023 || (mantissa != 0 && exponent < 1075))
                    count++;
            }
            return count;
        }

        [Benchmark]
        public int StringConversion()
        {
            int count = 0;
            foreach (var value in _testValues)
            {
                string str = value.ToString();
                if (str.Contains('.') || str.Contains(','))
                    count++;
            }
            return count;
        }

        [Benchmark]
        public int SubtractionCheck()
        {
            int count = 0;
            foreach (var value in _testValues)
            {
                if (Math.Abs(value - Math.Round(value)) > double.Epsilon)
                    count++;
            }
            return count;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("CASO 5: BENCHMARK DE ALGORITMOS");
            Console.WriteLine("==========================================\n");
            
            Console.WriteLine("Este benchmark compara diferentes métodos para detectar");
            Console.WriteLine("se um número double tem parte decimal.\n");
            
            Console.WriteLine("Executando benchmarks... (isso pode levar alguns minutos)\n");

            var summary = BenchmarkRunner.Run<HasDecimalsBenchmark>();

            Console.WriteLine("\n=== ANÁLISE DOS RESULTADOS ===");
            Console.WriteLine("\nObserve:");
            Console.WriteLine("1. Mean (tempo médio): Menor é melhor");
            Console.WriteLine("2. Allocated: Menos alocação é melhor");
            Console.WriteLine("3. Rank: 1 é o mais rápido");
            Console.WriteLine("\nConclusão:");
            Console.WriteLine("- Operações nativas (%, Math.Floor) são geralmente mais rápidas");
            Console.WriteLine("- Conversão de string é extremamente lenta");
            Console.WriteLine("- Bit manipulation pode ser rápida mas é complexa");
            
            Console.WriteLine("\n[Pressione qualquer tecla para sair]");
            Console.ReadKey();
        }
    }
}

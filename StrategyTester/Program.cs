using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace StrategyTester
{
	class Program
	{
		static void Main(string[] args)
		{
			//OptimizeFiles();
			TestTrandInvertion2();
			Console.ReadLine();
		}

		private static void TestTrandInvertion2()
		{
			var repository = new HistoryRepository();
			var analizer = new RepositoryAnalyzer(repository);
			Console.WriteLine(repository.Days.Count);

			for (int length = 1; length < 5; ++length)
			{
				for (int skipCount = 1; skipCount < 36; ++skipCount)
				{		
					var result = analizer.TestTrendInvertion(length, skipCount);
					Console.WriteLine("{4}, {0} {1}, {2}: {3}", length, result.Item2, result.Item1,
						result.Item2/((double) result.Item2 + result.Item1), skipCount);
				}
			}
		}

		private static void TestTrandInvertion()
		{
			var repository = new HistoryRepository();
			var analizer = new RepositoryAnalyzer(repository);

			for (int i = 1; i < 10; ++i)
			{
				var result = analizer.TestTrendInvertion(i);
				Console.WriteLine("{0} {1}, {2}: {3}", i, result.Item2, result.Item1,
					result.Item2 / ((double)result.Item2 + result.Item1));
			}
		}

		private static void TestCandlesInvertion()
		{
			var repository = new HistoryRepository();
			var analizer = new RepositoryAnalyzer(repository);

			for (int i = 1; i < 10; ++i)
			{
				var result = analizer.TestCandlesInvertion(i, true);
				Console.WriteLine("{0} {1}, {2}: {3}", i, result.Item2, result.Item1,
					result.Item2/((double) result.Item2 + result.Item1));

				result = analizer.TestCandlesInvertion(i, false);
				Console.WriteLine("{0} {1}, {2}: {3}", i, result.Item2, result.Item1,
					result.Item2 / ((double)result.Item2 + result.Item1));
			}
		}

		private static void OptimizeFiles()
		{
			var files = Directory.GetFiles(@"..\..\History\SBER-3.15", "*.txt");

			foreach (var filename in files)
			{
				HistoryReader.OptimizeFile(filename);
			}
		}

		private static void Test1()
		{
			var sw = new Stopwatch();

			sw.Start();
			var ticks = HistoryReader.ReadTicks("t150112_150121.txt", 100005, 210000);
			sw.Stop();

			//Console.WriteLine(sw.ElapsedMilliseconds);
			sw.Reset();
			//double maxDev = 0;
			//uint maxOffset = 0;

			//var middleDev = UniformDeviationStrategy.GetMiddleDiviation(ticks, 0, 110);
			//var cans = ticks.GroupBy(t => t.Date).Select(gr => gr.GroupBy(t => t.Time / UniformDeviationStrategy.TimeFrame).ToList()).ToList();
			var candles =
				ticks.GroupBy(t => t.Date)
					.SelectMany(
						gr0 =>
							gr0.GroupBy(t => (int) t.Time.TotalSeconds/UniformDeviationStrategy.TimeFrame).Select(gr => gr.ToList()).ToList())
					.ToList();

			var avg = candles.Average(c => Math.Abs(c.Last().Value - c.First().Value));
			sw.Start();

			for (uint offset = 170; offset <= 190; offset += 5)
			{
				for (int exitSize = 50; exitSize <= 70; exitSize += 5)
				{
					var result = UniformDeviationStrategy.TestTakeProfit(candles, offset, exitSize, 180, 90);
					Console.WriteLine("{0}, {1}: {2}", offset, exitSize, result);
				}
			}
			sw.Stop();
			Console.WriteLine(sw.ElapsedMilliseconds);
		}
	}
}

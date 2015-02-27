using System;
using System.Collections.Generic;
using System.IO;
using StrategyTester.Strategy;
using StrategyTester.Types;

namespace StrategyTester
{
	class Program
	{
		public const string ToolName = @"SBER-3.15";

		static void Main(string[] args)
		{
			//TestTrend();
			//var repository = new HistoryRepository("RTS-3.15", false);
			//var repository = new HistoryRepository("SPFB-3.15", false);
			//var repository = new HistoryRepository("SBRF-14", false);
			var repository = new HistoryRepository("RTS-14", false);

			Console.WriteLine(repository.Days.Count);

			TestExtremums(repository);
			//TestSber(repository);

			Console.WriteLine("End");
			Console.ReadLine();
		}

		private static void TestFuzzy()
		{
			for (int len = 10; len <= 24; len += 2)
			{
				for (int krfu = 3; krfu <= 20; ++krfu)
				{
					try
					{
						Console.WriteLine("{0} {1}: {2}", len, krfu, TrendPredictorTester.TestFuzzy("closes.txt", "pred_" + len + "_" + krfu + ".txt", len));
					}
					catch (Exception e)
					{
						break;
					}
				}
			}
		}

		private static void TestTrend()
		{
			for (int averageCount = 1; averageCount < 20; ++averageCount)
			{
				for (int shortAverageCount = 1; shortAverageCount < averageCount; ++shortAverageCount)
				{
					Console.WriteLine("{0} {1}: {2}", averageCount, shortAverageCount, TrendPredictorTester.TestAverage("closes.txt", averageCount, shortAverageCount));
				}
			}
		}

		private static void TestSber(HistoryRepository repository)
		{
			SberStrategy.Run(repository.Days);
		}

		private static void TestExtremums(HistoryRepository repository)
		{
			var resultText = new List<string>();

			for (int stop = 700; stop <= 1500; stop += 100)
			{
				for (int pegTopSize = 50; pegTopSize <= 140; pegTopSize += 10)
				{
					var strat = new ExtremumStrategy(stop, pegTopSize);

					var result = strat.Run(repository.Days);
					result.PrintDepo(@"depo\" + stop + "_" + pegTopSize + ".txt");

					resultText.Add("stop: " + stop + " pegTopSize: " + pegTopSize);
					resultText.Add(result.ToString());
					resultText.Add("");
				}
			}

			File.WriteAllLines("out.txt", resultText);
		}

		private static void TestExtremums0(HistoryRepository repository)
		{
			var resultText = new List<string>();

			for (int stop = 200; stop <= 1000; stop += 100)
			{
				var strat = new ExtremumStrategy(stop, 0);

				var result = strat.Run(repository.Days);
				result.PrintDepo(@"depo\" + stop + ".txt");

				resultText.Add("stop: " + stop);
				resultText.Add(result.ToString());
				resultText.Add("");
			}

			File.WriteAllLines("out.txt", resultText);
		}

		private static void TestTrandInvertion(HistoryRepository repository)
		{
			for (int length = 1; length < 5; ++length)
			{
				for (int skipCount = 1; skipCount < 2; ++skipCount)
				{		
					var result = ProbabilityAnalyzer.TestTrendInvertion(repository.Days, length, skipCount);
					Console.WriteLine("{4}, {0} {1}, {2}: {3}", length, result.Item2, result.Item1,
						result.Item2/((double) result.Item2 + result.Item1), skipCount);
				}
			}
		}

		private static void TestCandlesInvertionByDays(HistoryRepository repository)
		{
			int daysCount = repository.Days.Count; //5;

			for (int length = 1; length < 5; ++length)
			{
				for (int startDay = 0; startDay < repository.Days.Count; startDay += daysCount)
				{
					var tested = new List<Day>();

					for (int i = startDay; i < repository.Days.Count && i < startDay + daysCount; ++i)
					{
						tested.Add(repository.Days[i]);
					}
					var result = ProbabilityAnalyzer.TestCandlesInvertion(tested, length);
					Console.WriteLine("{0} {1}, {2}: {3}", length, result.Item2, result.Item1,
						result.Item2 / ((double)result.Item2 + result.Item1));
				}
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
	}
}

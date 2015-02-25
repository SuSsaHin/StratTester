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
			var repository = new HistoryRepository("RTS-14", false);

			Console.WriteLine(repository.Days.Count);

			TestExtremums2(repository);
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

		private static void TestExtremums(HistoryRepository repository)
		{
			var strat = new ExtremumStrategy();
			var resultText = new List<string>();

			for (int averageCount = 6; averageCount <= 15; averageCount++)
			{
				for (int stop = 400; stop <= 1000; stop += 100)
				{
					var result = strat.Run(repository.Days, stop, averageCount);
					result.PrintDepo(@"depo\" + averageCount + "_" + stop + ".txt");

					resultText.Add("AC: " + averageCount + "; stop: " + stop);
					resultText.Add(result.ToString());
				}
				//Console.WriteLine(averageCount);
				//Console.WriteLine(result);
				//File.WriteAllLines("ext.txt", strat.SExtremums);
				//result.PrintDeals();
			}
			File.WriteAllLines("out.txt", resultText);
			/*var result = strat.Run(repository.Days, 1000, 10);
			Console.WriteLine(result);
			result.PrintDeals("out.txt");
			result.PrintDepo("depo.txt");
			File.WriteAllLines("ext.txt", strat.SExtremums);*/
		}

		private static void TestExtremums2(HistoryRepository repository)
		{
			var strat = new ExtremumStrategy();
			var resultText = new List<string>();

			for (int stop = 400; stop <= 1000; stop += 100)
			{
				var result = strat.Run(repository.Days, stop);
				result.PrintDepo(@"depo\" + stop + ".txt");

				resultText.Add("stop: " + stop);
				resultText.Add(result.ToString());
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

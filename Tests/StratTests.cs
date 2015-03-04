using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using StrategyTester;
using StrategyTester.Strategy;

namespace Tests
{
	[TestClass]
	class StratTests
	{
		[TestCase("RTS-14",		600, 1000, 100, 40, 70, 10)]
		[TestCase("RTS-3.15",	900, 1400, 100, 40, 120, 20)]
		[TestCase("SBRF-14",	100, 300, 10, 3, 5, 1)]
		[TestCase("SBRF-3.15",	300, 1000, 30, 80)]
		[TestCase("SI-14",		100, 440, 40, 0, 3, 1)]
		[TestCase("SI-3.15",	200, 600, 50, 2, 2, 4)]
		public static void TestExtremums(string toolName, int startStop, int endStop, int stopStep, 
											int startPegTopSize, int endPegTopSize, int pegTopSizeStep)
		{
			var repository = new HistoryRepository(toolName, false);
			var resultText = new List<string>();
			//var good = new List<int>();
			//var bad = new List<int>();

			for (int stop = startStop; stop <= endStop; stop += stopStep)
			{
				for (int pegTopSize = startPegTopSize; pegTopSize <= endPegTopSize; pegTopSize += pegTopSizeStep)
				{
					for (double breakevenSize = 0.1; breakevenSize <= 0.25; breakevenSize += 0.05)
					{
						var strat = new ExtremumStrategy(stop, pegTopSize, breakevenSize);

						var result = strat.Run(repository.Days);
						result.PrintDepo(@"depo\" + stop + "_" + pegTopSize + "_" + (100*breakevenSize).ToString(new CultureInfo("en-us")) + ".txt");

						resultText.Add("stop: " + stop + " pegTopSize: " + pegTopSize + " breakevenSize: " + breakevenSize.ToString(new CultureInfo("en-us")));
						resultText.Add(result.ToString());
						resultText.Add("");
						//good.Add(strat.Good);
						//bad.Add(strat.Bad);
					}
				}
			}

			//var a = good.Average();
			//var b = bad.Average();

			File.WriteAllLines("out.txt", resultText);
		}

		[TestCase("RTS-14", 900, 60, 0.15)]
		[TestCase("RTS-3.15", 900, 60, 0.15)]
		[TestCase("SI-14", 260, 0, 0.2)]
		[TestCase("SI-3.15", 260, 0, 0.2)]
		public static void TestOptimumExtremums(string toolName, int stopLoss, int pegTopSize, double breakevenSize)
		{
			var repository = new HistoryRepository(toolName, false);
			var resultText = new List<string>();

			var strat = new ExtremumStrategy(stopLoss, pegTopSize, breakevenSize);

			for (int hours = 15; hours <= 23; ++hours)
			{
				for (int minutes = 0; minutes < 60; minutes += 30)
				{
					var result = strat.Run(repository.Days);
					result.PrintDepo(@"depo\" + hours + "_" + minutes + ".txt");

					resultText.Add(hours + "_" + minutes);
					resultText.Add(result.ToString());
					resultText.Add("");
				}
			}

			File.WriteAllLines("out.txt", resultText);
		}
		[TestCase("RTS-14", 600, 1000, 100, 30, 70, 10)]
		[TestCase("RTS-3.15", 500, 1800, 100, 50, 120, 10)]
		[TestCase("SBRF-14", 100, 300, 10, 3, 5, 1)]
		[TestCase("SBRF-3.15", 300, 1000, 30, 80)]
		[TestCase("SI-14", 100, 440, 40, 3, 3, 1)]
		[TestCase("SI-3.15", 200, 600, 50, 2, 8, 4)]
		public static void TestMultyExtremums(string toolName, int startStop, int endStop, int stopStep, int startPegTopSize, int endPegTopSize, int pegTopSizeStep)
		{
			var repository = new HistoryRepository(toolName, false);
			var resultText = new List<string>();
			//var good = new List<int>();
			//var bad = new List<int>();


			for (int minExtremumStep = 0; minExtremumStep <= 500; minExtremumStep += 100)
			{
				for (int stop = startStop; stop <= endStop; stop += stopStep)
				{
					for (int pegTopSize = startPegTopSize; pegTopSize <= endPegTopSize; pegTopSize += pegTopSizeStep)
					{
						var strat = new MultyExtremumStrategy(stop, pegTopSize, minExtremumStep);

						var result = strat.Run(repository.Days);
						result.PrintDepo(@"depo\" + stop + "_" + pegTopSize + ".txt");

						resultText.Add("mE: " + minExtremumStep + " stop: " + stop + " pegTopSize: " + pegTopSize);
						resultText.Add(result.ToString());
						resultText.Add("");
						//good.Add(strat.Good);
						//bad.Add(strat.Bad);
					}
				}
			}

			//var a = good.Average();
			//var b = bad.Average();

			File.WriteAllLines("out.txt", resultText);
		}

		[TestCase("RTS-14")]
		[TestCase("RTS-3.15")]
		[TestCase("SBRF-14")]
		[TestCase("SBRF-3.15")]
		public static void TestExtremumsSimple(string toolName)
		{
			var repository = new HistoryRepository(toolName, false);
			var resultText = new List<string>();

			for (int stop = 200; stop <= 1000; stop += 100)
			{
				var strat = new ExtremumStrategy(stop, 0, 50);

				var result = strat.Run(repository.Days);
				result.PrintDepo(@"depo\" + stop + ".txt");

				resultText.Add("stop: " + stop);
				resultText.Add(result.ToString());
				resultText.Add("");
			}

			File.WriteAllLines("out.txt", resultText);
		}

		[TestCase("RTS-14")]
		[TestCase("SBRF-14")]
		[TestCase("SI-14")]
		public static void TestTrendFromNCandle(string toolName)
		{
			var repository = new HistoryRepository(toolName, false);
			//File.WriteAllLines("ClosesSber.txt", repository.Days.Select(day => day.Params.Close.ToString()));
			var maxCount = repository.Days.Min(day => day.FiveMins.Count);
			var result = new List<string>();
			for (int i = 0; i < maxCount; ++i)
			{
				result.Add(ProbabilityAnalyzer.TestTrendFromNCandle(repository.Days, i).ToString(new CultureInfo("en-us")));
			}
			File.WriteAllLines("Prob.txt", result);
		}

		[Test]
		public static void TestFuzzy()
		{
			for (int len = 10; len <= 24; len += 4)
			{
				for (int krfu = 3; krfu <= 20; krfu+=4)
				{
					try
					{
						Console.WriteLine("{0} {1}: {2}", len, krfu, TrendPredictorTester.TestFuzzy("closesSber.txt", "pred_" + len + "_" + krfu + ".txt", len));
					}
					catch (Exception ex)
					{
						break;
					}
				}
			}
		}

		[Test]
		public static void TestTrend()
		{
			for (int averageCount = 1; averageCount < 20; ++averageCount)
			{
				for (int shortAverageCount = 1; shortAverageCount < averageCount; ++shortAverageCount)
				{
					Console.WriteLine("{0} {1}: {2}", averageCount, shortAverageCount, TrendPredictorTester.TestAverage("closes.txt", averageCount, shortAverageCount));
				}
			}
		}

		[TestCase("SBRF-14")]
		[TestCase("SBRF-3.15")]
		[TestCase("SI-14")]
		[TestCase("SI-3.15")]
		public static void PrintGraphs(string toolName)
		{
			var repository = new HistoryRepository(toolName, false);
			InformationPrinter.Run(toolName, repository.Days);
		}
	}
}

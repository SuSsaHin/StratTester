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
		[TestCase("RTS-14")]
		[TestCase("RTS-3.15")]
		[TestCase("SBRF-14")]
		[TestCase("SBRF-3.15")]
		public static void TestExtremums(string toolName)
		{
			var repository = new HistoryRepository(toolName, false);
			var resultText = new List<string>();
			//var good = new List<int>();
			//var bad = new List<int>();

			for (int stop = 300; stop <= 1000; stop += 100)
			{
				for (int pegTopSize = 30; pegTopSize <= 80; pegTopSize += 10)
				{
					var strat = new ExtremumStrategy(stop, pegTopSize);

					var result = strat.Run(repository.Days);
					result.PrintDepo(@"depo\" + stop + "_" + pegTopSize + ".txt");

					resultText.Add("stop: " + stop + " pegTopSize: " + pegTopSize);
					resultText.Add(result.ToString());
					resultText.Add("");
					//good.Add(strat.Good);
					//bad.Add(strat.Bad);
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
		public static void TestExtremumsSimpe(string toolName)
		{
			var repository = new HistoryRepository(toolName, false);
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

		[TestCase("RTS-14")]
		[TestCase("SBRF-14")]
		public static void TestTrendFromNCandle(string toolName)
		{
			var repository = new HistoryRepository(toolName, false);
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
			for (int len = 10; len <= 24; len += 2)
			{
				for (int krfu = 3; krfu <= 20; ++krfu)
				{
					try
					{
						Console.WriteLine("{0} {1}: {2}", len, krfu, TrendPredictorTester.TestFuzzy("closes.txt", "pred_" + len + "_" + krfu + ".txt", len));
					}
					catch (Exception)
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
		public static void TestSber(string toolName)
		{
			var repository = new HistoryRepository(toolName, false);
			InformationPrinter.Run(repository.Days);
		}
	}
}

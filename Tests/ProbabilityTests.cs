using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using StrategyTester;

namespace Tests
{
	[TestClass]
	class ProbabilityTests
	{
		[TestCase("RTS-14")]
		[TestCase("SBRF-14")]
		[TestCase("SI-14")]
		public static void TestExtremumsContinuation(string toolName)
		{
			const int maxCount = 30;
			var repository = new HistoryRepository(toolName, false);

			var result = new List<Tuple<int, int>>();
			for (int i = 2; i < maxCount; ++i)
			{
				int successCount = 0, failCOunt = 0;
				foreach (var day in repository.Days)
				{
					var current = ProbabilityAnalyzer.TestExtremumsContinuation(day.FiveMins, i, true);
					successCount += current.Item1;
					failCOunt += current.Item2;
				}
				result.Add(new Tuple<int, int>(successCount, failCOunt));
			}
			File.WriteAllLines("continProbs.txt", result.ConvertAll(t => (t.Item1 + "\t" + t.Item2)));
		}

		[TestCase("RTS-14")]
		[TestCase("SBRF-14")]
		[TestCase("SI-14")]
		public static void TestExtremumsContinuationLength(string toolName)
		{
			const int maxCount = 30;
			const bool isTrendLong = true;
			var repository = new HistoryRepository(toolName, false);

			var averages = new List<List<int>>();
			for (int i = 1; i < maxCount; ++i)
			{
				var lengths = new List<int>();
				foreach (var day in repository.Days)
				{
					var current = ProbabilityAnalyzer.TestExtremumsContinuationLength(day.FiveMins, i, isTrendLong);
					lengths.AddRange(current);
				}

				if (!lengths.Any())
					lengths.Add(0);

				averages.Add(lengths);
			}
			File.WriteAllLines("continLengthProbs.txt", averages.ConvertAll(list => list.Average().ToString(new CultureInfo("en-us")) + "\t" + list.Count));
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
				for (int krfu = 3; krfu <= 20; krfu += 4)
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
	}
}

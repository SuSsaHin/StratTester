using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using StrategyTester;
using StrategyTester.Utils;

namespace Tests
{
	[TestClass]
	class ProbabilityTests
	{
        [TestCase("RTS-14")]
        [TestCase("RTS-15")]
        public static void TestDaysHeightsAlternation(string toolName)
        {
            const int maxContinuationLength = 5;
            const int maxAverageDaysLength = 20;

            var repository = new HistoryRepository(toolName, false);

            var output = new List<string>();

            var headers = new List<string> { "Length of average interval", "Continuation length", "Alternates count", "Continuation count", "Alternates percent" };
            var tableOutput = new List<List<string>>();
            for (int averageDaysLength = 5; averageDaysLength <= maxAverageDaysLength; ++averageDaysLength)
            {
                output.Add("Length of average interval: " + averageDaysLength);
                output.Add("");
                for (int continuationLength = 1; continuationLength < maxContinuationLength; ++continuationLength)
                {
                    var current = ProbabilityAnalyzer.TesCandlesHightAlternation(repository.Days.Select(day => day.Params).ToList(), continuationLength, averageDaysLength);
                    decimal alternatePercent = Math.Round(current.Item1/((decimal) current.Item1 + current.Item2), 2);
                    output.Add("Continuation length: " + continuationLength);
                    output.Add("Alternates count: " + current.Item1 + ", continuation count: " + current.Item2 + ", alternates percent: " + alternatePercent);
                    output.Add("");
                    tableOutput.Add(new List<string>{averageDaysLength.ToString(), continuationLength.ToString(), current.Item1.ToString(), current.Item2.ToString(), alternatePercent.ToString(new CultureInfo("en-us"))});
                }
                output.Add("");
            }
            TablesWriter.PrintExcel("AlternationTest " + toolName + ".xlsx", headers, tableOutput);
            File.WriteAllLines("AlternationTest " + toolName + ".txt", output);
        }

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
		[TestCase("RTS-3.15")]
		[TestCase("SBRF-14")]
		[TestCase("SI-14")]
		[TestCase("SI-3.15")]
		public static void TestExtremumsContinuationFronAngle(string toolName)
		{
			const int maxCount = 15;
			var repository = new HistoryRepository(toolName, false);

			var result = new List<Tuple<int, int>>();
			for (int monotoneCount = 2; monotoneCount < maxCount; ++monotoneCount)
			{
				result.Clear();
				for (double minAngle = 0; minAngle < Math.PI/2; minAngle += 0.1)
				{
					int successCount = 0, failCount = 0;
					foreach (var day in repository.Days)
					{
						var current = ProbabilityAnalyzer.TestExtremumsContinuationFromAngle(day.FiveMins, monotoneCount, minAngle, true);
						successCount += current.Item1;
						failCount += current.Item2;
					}
					if (failCount == 0)
						failCount++;

					result.Add(new Tuple<int, int>(successCount, failCount));
				}
				File.WriteAllLines("continProbs" + monotoneCount + ".txt",
					result.ConvertAll(t => (t.Item1 + "\t" + t.Item2)));
					//result.ConvertAll(t => (t.Item1/((double) t.Item2 + t.Item1)).ToString(new CultureInfo("en-us"))));
			}
			
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
			for (int i = 1; i < maxCount; ++i)
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using StrategyTester;
using StrategyTester.Strategy;
using StrategyTester.Utils;

namespace Tests
{
	[TestClass]
	class ExtremumStrategyTests
	{
		[TestCase("RTS-14",		600, 1000, 100, 60, 60, 10)]
		[TestCase("RTS-3.15",	900, 1400, 100, 40, 120, 20)]
		[TestCase("SBRF-14",	100, 300, 10, 3, 5, 1)]
		[TestCase("SBRF-3.15",	300, 1000, 30, 80)]
		[TestCase("SI-14",		100, 440, 40, 0, 3, 1)]
		[TestCase("SI-3.15",	250, 450, 50, 10, 30, 10)]
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
					for (double breakevenSize = 0.8; breakevenSize <= 2.0; breakevenSize += 0.1)
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

        //[TestCase("RTS-15", 700, 1000, 100, 700, 1000, 100, 70)]
        [TestCase("RTS-15", 600, 1000, 200, 600, 1000, 200, 1000, 4000, 500, 70)]
        [TestCase("RTS-14", 600, 1000, 200, 600, 1000, 200, 1000, 4000, 500, 70)]
        public static void TestCorrectedExtremums(string toolName, int startStop, int endStop, int stopStep,
                                             int startTrStop, int endTrStop, int trStopStep,
                                             int startMaxDistFromOpen, int endMaxDistFromOpen, int maxDistFromOpenStep,
                                             int pegTopSize)
        {
            var repository = new HistoryRepository(toolName, false);
            var resultText = new List<string>();

            for (int stop = startStop; stop <= endStop; stop += stopStep)
            for (int trStop = Math.Max(startTrStop, stop); trStop <= endTrStop; trStop += trStopStep)
            for (double breakevenSize = 0.0; breakevenSize <= 0.61; breakevenSize += 0.2)
            for (int maxDistFromOpen = startMaxDistFromOpen; maxDistFromOpen <= endMaxDistFromOpen; maxDistFromOpen += maxDistFromOpenStep)
            {
                var strat = new CorrectedExtremumStrategy(stop, pegTopSize, breakevenSize, trStop, maxDistFromOpen);

                var result = strat.Run(repository.Days);
                result.PrintDepo(@"depo\" + stop + "_" + trStop + "_" +
                                    (100*breakevenSize).ToString(new CultureInfo("en-us")) + "_" + maxDistFromOpen + ".txt");

                resultText.Add("stop: " + stop + " trailingStop: " + trStop + " breakevenSize: " +
                                breakevenSize.ToString(new CultureInfo("en-us")) + " maxDistFromOpen: " + maxDistFromOpen);
                resultText.Add(result.ToString());
                resultText.Add("");
            }

            File.WriteAllLines("out.txt", resultText);
        }

		[TestCase("RTS-14", 900, 60, 0.15)]
		[TestCase("RTS-3.15", 900, 60, 0.15)]
		public static void TestTrailingCorrectedExtremums(string toolName, int stopLoss, int pegTopSize, double breakevenSize)
		{
			var repository = new HistoryRepository(toolName, false);
			var resultText = new List<string>();

			for (int trailingSize = 700; trailingSize <= 1800; trailingSize += 100)
			{
				var strat = new CorrectedExtremumStrategy(stopLoss, pegTopSize, breakevenSize, trailingSize);

				var result = strat.Run(repository.Days);
				result.PrintDepo(@"depo\" + trailingSize + ".txt");

				resultText.Add(trailingSize.ToString());
				resultText.Add(result.ToString());
				resultText.Add("");
			}

			File.WriteAllLines("out.txt", resultText);
		}

		[TestCase("RTS-14", 900, 60, 0.15)]
		[TestCase("RTS-3.15", 900, 60, 0.15)]
		[TestCase("SI-14", 260, 0, 0.2)]
		[TestCase("SI-3.15", 350, 20, 0.3)]
		public static void TestEndTimeExtremums(string toolName, int stopLoss, int pegTopSize, double breakevenSize)
		{
			var repository = new HistoryRepository(toolName, false);
			var resultText = new List<string>();

			for (int hours = 15; hours <= 23; ++hours)
			{
				for (int minutes = 0; minutes < 60; minutes += 15)
				{
					var time = new TimeSpan(hours, minutes, 0);
					var strat = new ExtremumStrategy(stopLoss, pegTopSize, breakevenSize, time);

					var result = strat.Run(repository.Days);
					result.PrintDepo(@"depo\" + hours + "_" + minutes + ".txt");

					resultText.Add(hours + "_" + minutes);
					resultText.Add(result.ToString());
					resultText.Add("");
				}
			}

			File.WriteAllLines("out.txt", resultText);
		}

		[TestCase("RTS-14", 900, 60, 0.15)]
		[TestCase("RTS-3.15", 900, 60, 0.15)]
		[TestCase("SI-14", 260, 0, 0.2)]
		[TestCase("SI-3.15", 350, 20, 0.3)]
		public static void RunExtremums(string toolName, int stopLoss, int pegTopSize, double breakevenSize)
		{
			var repository = new HistoryRepository(toolName, false);
			var resultText = new List<string>();

			/*var date = new DateTime(2014, 06, 01);
			var avgCandles = repository.Days.SelectMany(day => day.FiveMins).ToList();
			var avg = avgCandles.Average(c => Math.Abs(c.Close - c.Open));*/

			var strat = new ExtremumStrategy(stopLoss, pegTopSize, breakevenSize);

			var result = strat.Run(repository.Days);
			result.PrintDepo(@"depo\run.txt");

			resultText.Add(result.ToString());
			resultText.Add("");

			File.WriteAllLines("out.txt", resultText);
		}

		[TestCase("RTS-14", 900, 60, 0.15)]
		[TestCase("RTS-3.15", 900, 60, 0.15)]
		[TestCase("SI-14", 260, 0, 0.2)]
		[TestCase("SI-3.15", 350, 20, 0.3)]
		public static void RunCorrectedExtremums(string toolName, int stopLoss, int pegTopSize, double breakevenSize)
		{
			var repository = new HistoryRepository(toolName, false);
			var resultText = new List<string>();

			var strat = new CorrectedExtremumStrategy(stopLoss, pegTopSize, breakevenSize, 900);

			var result = strat.Run(repository.Days);
			result.PrintDepo(@"depo\run.txt");

			resultText.Add(result.ToString());
			resultText.Add("");

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

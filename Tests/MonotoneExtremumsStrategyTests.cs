using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using StrategyTester;
using StrategyTester.Strategy;

namespace Tests
{
	[TestClass]
	class MonotoneExtremumsStrategyTests
	{
		[TestCase("RTS-14", 500, 1000, 100)]
		[TestCase("RTS-3.15", 900, 1400, 100)]
		[TestCase("SI-14", 260, 1000, 100)]
		[TestCase("SI-3.15", 350, 1000, 100)]
		public static void OptimizeAll(string toolName, int startStop, int endStop, int stopStep)
		{
			var repository = new HistoryRepository(toolName, false);
			var resultText = new List<string>();
			const double breakevenPercent = 0.15;
			var lastTradeTime = new TimeSpan(15, 45, 00);

			for (int monotoneCount = 3; monotoneCount <= 4; ++monotoneCount)
			{
				for (int invertCount = 2; invertCount <= monotoneCount; ++invertCount)
				{
					for (int stopLoss = startStop; stopLoss <= endStop; stopLoss += stopStep)
					{
						var strat = new MonotoneExtremumsStrategy(monotoneCount, stopLoss, invertCount, breakevenPercent, lastTradeTime);

						var result = strat.Run(repository.Days);
						result.PrintDepo(@"mono\depo\" + monotoneCount + "_" + invertCount + "_" + stopLoss + ".txt");

						if (result.DealsCount == 0)
							continue;

						resultText.Add(monotoneCount + " " + invertCount + " " + stopLoss);
						resultText.Add(result.ToString());
						resultText.Add("");
					}
				}
			}
			File.WriteAllLines(@"mono\out.txt", resultText);
		}

		[TestCase("RTS-14", 800, 4, 3)]
		public static void OptimizeEndTime(string toolName, int stopLoss, int monotoneCount, int invertCount)
		{
			var repository = new HistoryRepository(toolName, false);
			var resultText = new List<string>();

			for (int hours = 12; hours <= 23; ++hours)
			{
				for (int minutes = 0; minutes < 60; minutes += 15)
				{
					var time = new TimeSpan(hours, minutes, 0);
					var strat = new MonotoneExtremumsStrategy(monotoneCount, stopLoss, invertCount, 0.15, time);

					var result = strat.Run(repository.Days);
					result.PrintDepo(@"mono\depo\" + hours + "_" + minutes + ".txt");

					resultText.Add(hours + "_" + minutes);
					resultText.Add(result.ToString());
					resultText.Add("");
				}
			}

			File.WriteAllLines(@"mono\out.txt", resultText);
		}

		[TestCase("RTS-14", 900, 60, 0.15)]
		[TestCase("RTS-3.15", 900, 60, 0.15)]
		[TestCase("SI-14", 260, 0, 0.2)]
		[TestCase("SI-3.15", 350, 20, 0.3)]
		public static void RunMonotoneExtremums(string toolName, int stopLoss, int pegTopSize, double breakevenSize)
		{
			var repository = new HistoryRepository(toolName, false);
			var resultText = new List<string>();

			var strat = new MonotoneExtremumsStrategy(5, stopLoss, 3, 0.2);

			var result = strat.Run(repository.Days);
			result.PrintDepo(@"mono\depo\run.txt");

			resultText.Add(result.ToString());
			resultText.Add("");

			File.WriteAllLines(@"mono\out.txt", resultText);
		}
	}
}

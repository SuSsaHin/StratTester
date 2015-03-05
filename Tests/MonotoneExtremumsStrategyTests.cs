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
		[TestCase("RTS-14", 900, 60, 0.15)]
		[TestCase("RTS-3.15", 900, 60, 0.15)]
		[TestCase("SI-14", 260, 0, 0.2)]
		[TestCase("SI-3.15", 350, 20, 0.3)]
		public static void TestMonotoneExtremums(string toolName, int stopLoss, int pegTopSize, double breakevenSize)
		{
			var repository = new HistoryRepository(toolName, false);
			var resultText = new List<string>();

			for (int monotoneCount = 2; monotoneCount < 10; ++monotoneCount)
			{
				for (int invertCount = 1; invertCount <= monotoneCount; ++invertCount)
				{
					var strat = new MonotoneExtremumsStrategy(monotoneCount, stopLoss, invertCount);

					var result = strat.Run(repository.Days);
					result.PrintDepo(@"mono\depo\i.txt");

					if (result.DealsCount == 0)
						continue;

					resultText.Add(monotoneCount + " " + invertCount);
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

			var strat = new MonotoneExtremumsStrategy(5, stopLoss, 3);

			var result = strat.Run(repository.Days);
			result.PrintDepo(@"mono\depo\run.txt");

			resultText.Add(result.ToString());
			resultText.Add("");

			File.WriteAllLines(@"mono\out.txt", resultText);
		}
	}
}

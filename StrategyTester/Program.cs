using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using StrategyTester.Strategy;
using StrategyTester.Types;

namespace StrategyTester
{
	class Program
	{
		public const string ToolName = @"SBER-3.15";

		static void Main(string[] args)
		{

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

		private static void OptimizeTicksFiles()
		{
			var files = Directory.GetFiles(@"..\..\History\SBER-3.15", "*.txt");

			foreach (var filename in files)
			{
				HistoryReader.OptimizeFile(filename);
			}
		}
	}
}

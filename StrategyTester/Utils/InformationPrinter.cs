using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester.Utils
{
	public static class InformationPrinter
	{
		//private const string outPath = @"sber\";
		private const string candlesPath = @"candles\";
		private const string firstMaxPath = @"fMax\";
		private const string firstMinPath = @"fMin\";
		private const string secondMaxPath = @"sMax\";
		private const string secondMinPath = @"sMin\";

		private const int pegTopSize = 0;

		public static void Run(string toolName, List<Day> days)
		{
			var outPath = toolName + @"\";

			Directory.CreateDirectory(outPath + candlesPath);
			Directory.CreateDirectory(outPath + firstMaxPath);
			Directory.CreateDirectory(outPath + firstMinPath);
			Directory.CreateDirectory(outPath + secondMaxPath);
			Directory.CreateDirectory(outPath + secondMinPath);

			var extremumsFindex = new ExtremumsFinder(pegTopSize);
			
			for (int i = 0; i < days.Count; ++i)
			{
				var day = days[i];
				var candles = day.FiveMins;
				var firstMaximums = extremumsFindex.FindFirstExtremums(candles, false);
				var firstMinimums = extremumsFindex.FindFirstExtremums(candles, true);

				var secondMaximums = extremumsFindex.FindSecondExtremums(firstMaximums, false);
				var secondMinimums = extremumsFindex.FindSecondExtremums(firstMinimums, true);

				File.WriteAllLines(outPath + candlesPath + i + ".txt", candles.ConvertAll(c => c.High + "\t" + c.Low + "\t" + c.Close + "\t" + c.Open));

				File.WriteAllLines(outPath + firstMaxPath + i + ".txt", firstMaximums.Select(ex => (candles.FindIndex(c => c.Time == ex.Date.TimeOfDay) + 1).ToString()));
				File.WriteAllLines(outPath + firstMinPath + i + ".txt", firstMinimums.Select(ex => (candles.FindIndex(c => c.Time == ex.Date.TimeOfDay) + 1).ToString()));

				File.WriteAllLines(outPath + secondMaxPath + i + ".txt", secondMaximums.Select(ex => (candles.FindIndex(c => c.Time == ex.Date.TimeOfDay) + 1).ToString()));
				File.WriteAllLines(outPath + secondMinPath + i + ".txt", secondMinimums.Select(ex => (candles.FindIndex(c => c.Time == ex.Date.TimeOfDay) + 1).ToString()));
			}
		}
	}
}

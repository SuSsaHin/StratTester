using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StrategyTester.Types;
using StrategyTester.Utils;

namespace StrategyTester.Strategy
{
	public static class InformationPrinter
	{
		private const string outPath = @"sber\";
		private const string candlesPath = @"candles\";
		private const string firstMaxPath = @"fMax\";
		private const string firstMinPath = @"fMin\";
		private const string secondMaxPath = @"sMax\";
		private const string secondMinPath = @"sMin\";

		private const int pegTopSize = 3;

		public static void Run(List<Day> days)
		{
			for (int i = 0; i < days.Count; ++i)
			{
				var day = days[i];
				var candles = day.FiveMins;
				var firstMaximums = FindFirstExtremums(candles, false);
				var firstMinimums = FindFirstExtremums(candles, true);

				var secondMaximums = FindSecondExtremums(firstMaximums, false);
				var secondMinimums = FindSecondExtremums(firstMinimums, true);

				File.WriteAllLines(outPath + candlesPath + i + ".txt", candles.ConvertAll(c => c.High + "\t" + c.Low + "\t" + c.Close + "\t" + c.Open));

				File.WriteAllLines(outPath + firstMaxPath + i + ".txt", firstMaximums.Select(ex => (candles.FindIndex(c => c.Time == ex.Date.TimeOfDay) + 1).ToString()));
				File.WriteAllLines(outPath + firstMinPath + i + ".txt", firstMinimums.Select(ex => (candles.FindIndex(c => c.Time == ex.Date.TimeOfDay) + 1).ToString()));

				File.WriteAllLines(outPath + secondMaxPath + i + ".txt", secondMaximums.Select(ex => (candles.FindIndex(c => c.Time == ex.Date.TimeOfDay) + 1).ToString()));
				File.WriteAllLines(outPath + secondMinPath + i + ".txt", secondMinimums.Select(ex => (candles.FindIndex(c => c.Time == ex.Date.TimeOfDay) + 1).ToString()));
			}
		}

		private static List<Extremum> FindAllSecondExtremums(List<Candle> daysCandles)
		{
			var firstLongExtremums = FindFirstExtremums(daysCandles, true);
			var firstShortExtremums = FindFirstExtremums(daysCandles, false);

			return FindSecondExtremums(firstLongExtremums, true)
				.Concat(FindSecondExtremums(firstShortExtremums, false))
				.OrderBy(ex => ex.Date)
				.ToList();
		}

		private static List<Extremum> FindAllFirstExtremums(List<Candle> daysCandles)
		{
			var firstLongExtremums = FindFirstExtremums(daysCandles, true);
			var firstShortExtremums = FindFirstExtremums(daysCandles, false);

			return firstLongExtremums
				.Concat(firstShortExtremums)
				.OrderBy(ex => ex.Date)
				.ToList();
		}

		private static bool IsPegTop(Candle candle)
		{
			return Math.Abs(candle.Open - candle.Close) <= pegTopSize;
		}

		private static List<Extremum> FindFirstExtremums(List<Candle> candles, bool isMinimum)
		{
			var extremums = new List<Extremum>();
			for (int leftIndex = 0; leftIndex < candles.Count; ++leftIndex)
			{
				var leftCandle = candles[leftIndex];
				var midIndex = candles.FindIndex(leftIndex + 1, c => !c.IsOuter(leftCandle));
				if (midIndex == -1)
					continue;

				var midCandle = candles[midIndex];

				if (isMinimum && midCandle.Low > leftCandle.Low || !isMinimum && midCandle.High < leftCandle.High)
					continue;

				var rightIndex = candles.FindIndex(midIndex + 1, c => !c.IsInner(midCandle));
				if (rightIndex == -1)
					continue;

				var rightCandle = candles[rightIndex];

				if (isMinimum && midCandle.Low > rightCandle.Low || !isMinimum && midCandle.High < rightCandle.High)
					continue;

				if (IsPegTop(leftCandle) && IsPegTop(midCandle) && IsPegTop(rightCandle))
					continue;

				var extremum = new Extremum(isMinimum ? midCandle.Low : midCandle.High, rightIndex, midCandle.Date + midCandle.Time, isMinimum);
				/*if (extremums.Any() && extremum.Date == extremums[extremums.Count - 1].Date)
				{
					continue;
				}*/

				extremums.Add(extremum);
				//leftIndex = midIndex;
			}

			return extremums;
		}

		private static List<Extremum> FindSecondExtremums(List<Extremum> firstExtremums, bool isMinimum)
		{
			var exremums = new List<Extremum>();
			for (int i = 1; i < firstExtremums.Count - 1; ++i)
			{
				var currentValue = firstExtremums[i].Value;
				var previousExtremum = firstExtremums[i - 1];
				/*int j = i - 1;
				while (j > 0 && previousExtremum.Value == currentValue)
				{
					previousExtremum = firstExtremums[--j];
				}*/

				if (isMinimum && currentValue < previousExtremum.Value &&
					currentValue < firstExtremums[i + 1].Value ||
					!isMinimum && currentValue > previousExtremum.Value &&
					currentValue > firstExtremums[i + 1].Value)
				{
					exremums.Add(new Extremum(currentValue, firstExtremums[i + 1].CheckerIndex, firstExtremums[i].Date, isMinimum));
				}
			}

			return exremums;
		}
	}
}

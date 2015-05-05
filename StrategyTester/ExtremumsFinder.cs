using System;
using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;
using StrategyTester.Utils;

namespace StrategyTester
{
    public class ExtremumsFinder
	{
		private readonly int pegTopSize;

		public ExtremumsFinder(int pegTopSize)
		{
			this.pegTopSize = pegTopSize;
		}

		public List<Extremum> FindFirstExtremums(List<Candle> candles, bool isMinimum)
		{
			var extremums = new List<Extremum>();
			for (int leftIndex = 0; leftIndex < candles.Count; ++leftIndex)
			{
				var leftCandle = candles[leftIndex];
				var midIndex = candles.FindIndex(leftIndex + 1, c => !c.IsOuter(leftCandle));
//                var midIndex = candles.FindIndex(leftIndex + 1, c => !leftCandle.IsOuter(c) && !leftCandle.IsInner(c));
				if (midIndex == -1)
					continue;

				var midCandle = candles[midIndex];

				if (isMinimum && midCandle.Low > leftCandle.Low || !isMinimum && midCandle.High < leftCandle.High)
					continue;

				var rightIndex = candles.FindIndex(midIndex + 1, c => !c.IsInner(midCandle));
//                var rightIndex = candles.FindIndex(midIndex + 1, c => !midCandle.IsInner(c) && !midCandle.IsOuter(c));
                if (rightIndex == -1)
					continue;

				var rightCandle = candles[rightIndex];

				if (isMinimum && midCandle.Low > rightCandle.Low || !isMinimum && midCandle.High < rightCandle.High)
					continue;

				if (IsPegTop(leftCandle) && IsPegTop(midCandle) && IsPegTop(rightCandle))
					continue;

				var extremum = new Extremum(isMinimum ? midCandle.Low : midCandle.High, rightIndex, midCandle.Date + midCandle.Time, isMinimum);
				if (extremums.Any() && extremums[extremums.Count - 1].Date == extremum.Date)
				{
					extremums[extremums.Count - 1].CanBeSecond = false;
					continue;
				}

				if (extremums.Any() && extremums[extremums.Count - 1].Date > extremum.Date)
					continue;

				extremums.Add(extremum);
			}

			return extremums;
		}

		public List<Extremum> FindSecondExtremums(List<Extremum> firstExtremums, bool isMinimum)
		{
			var exremums = new List<Extremum>();
			for (int i = 1; i < firstExtremums.Count - 1; ++i)
			{
				if (!firstExtremums[i].CanBeSecond)
					continue;

				var currentValue = firstExtremums[i].Value;
				var previousExtremum = firstExtremums[i - 1];

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

		public List<Extremum> FindAllFirstExtremums(List<Candle> daysCandles)
		{
			var firstLongExtremums = FindFirstExtremums(daysCandles, true);
			var firstShortExtremums = FindFirstExtremums(daysCandles, false);

			return MergeExtremums(firstLongExtremums, firstShortExtremums);
		}

		public List<Extremum> FindAllSecondExtremums(List<Candle> daysCandles)
		{
			var firstLongExtremums = FindFirstExtremums(daysCandles, true);
			var firstShortExtremums = FindFirstExtremums(daysCandles, false);

			var secondLongExtremums = FindSecondExtremums(firstLongExtremums, true);
			var secondShortExtremums = FindSecondExtremums(firstShortExtremums, false);

			return MergeExtremums(secondLongExtremums, secondShortExtremums);
		}

		public List<Extremum> MergeExtremums(IEnumerable<Extremum> extremums1, IEnumerable<Extremum> extremums2)
		{
			var allExtremums = extremums1
				.Concat(extremums2)
				.OrderBy(ex => ex.CheckerIndex)
				.ToList();

			for (int i = 0; i < allExtremums.Count - 1; ++i)
			{
				if (allExtremums[i].Date >= allExtremums[i + 1].Date)
				{
					allExtremums.RemoveAt(i + 1);
					--i;
				}
			}

			return allExtremums;
		}

		private bool IsPegTop(Candle candle)
		{
			return Math.Abs(candle.Open - candle.Close) <= pegTopSize;
		}
	}
}

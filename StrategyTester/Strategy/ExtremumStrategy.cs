using System;
using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;
using StrategyTester.Utils;

namespace StrategyTester.Strategy
{
	public class ExtremumStrategy
	{
		private readonly int stopLoss;
		private readonly int pegTopSize;

		public int Good, Bad;

		public ExtremumStrategy(int stopLoss, int pegTopSize)
		{
			this.stopLoss = stopLoss;
			this.pegTopSize = pegTopSize;
		}

		public TradesResult Run(List<Day> days)
		{
			var result = new TradesResult();

			foreach (var day in days)
			{
				var profit = GetDaysDeal(day.FiveMins);
				if (profit == null)
					continue;

				result.AddDeal(profit);
			}

			return result;
		}

		private bool IsPegTop(Candle candle)
		{
			return Math.Abs(candle.Open - candle.Close) <= pegTopSize;
		}

		private Deal GetDaysDeal(List<Candle> daysCandles)
		{
			var allExtremums = FindAllSecondExtremums(daysCandles);

			foreach (var extremum in allExtremums)
			{
				var startIndex = extremum.CheckerIndex;
				if (startIndex == daysCandles.Count-1)
					break;

				var startPrice = daysCandles[startIndex + 1].Open;

				bool isTrendLong = startPrice > daysCandles.First().Open;

				if (isTrendLong != extremum.IsMinimum)
				{
					if (isTrendLong == (daysCandles[daysCandles.Count - 1].Close > startPrice))
					{
						++Good;
					}
					else
					{
						++Bad;
					}
					continue;
				}

				/*if (!isTrendLong)
					continue;*/

				var dynamicStopLoss = GetDynamicStopLoss(startPrice, isTrendLong, extremum);

				var stopResult = GetStopResult(daysCandles.Skip(startIndex + 1), isTrendLong, dynamicStopLoss);

				var endPrice = stopResult != -1 ? stopResult : daysCandles[daysCandles.Count - 1].Close;

				return new Deal(startPrice, endPrice, isTrendLong);
			}
			return null;
		}

		private int GetProfit(bool isTrendLong, int startPrice, int endPrice)
		{
			return isTrendLong ? endPrice - startPrice : startPrice - endPrice;
		}

		private int GetDynamicStopLoss(int startPrice, bool isTrendLong, Extremum extremum)
		{
			/*const double maxStopRatio = 1.5;
			var dist = -GetProfit(isTrendLong, startPrice, extremum.Value);

			if (dist > stopLoss && dist < stopLoss*maxStopRatio)
				return dist;*/

			return stopLoss;
		}

		private List<Extremum> FindAllSecondExtremums(List<Candle> daysCandles)
		{
			var firstLongExtremums = FindFirstExtremums(daysCandles, true);
			var firstShortExtremums = FindFirstExtremums(daysCandles, false);

			return FindSecondExtremums(firstLongExtremums, true)
				.Concat(FindSecondExtremums(firstShortExtremums, false))
				.OrderBy(ex => ex.Date)
				.ToList();
		}

		private List<Extremum> FindFirstExtremums(List<Candle> candles, bool isMinimum)
		{
			var extremums = new List<Extremum>();
			for (int leftIndex = 0; leftIndex < candles.Count; ++leftIndex)
			{
				var leftCandle = candles[leftIndex];
				var midIndex = candles.FindIndex(leftIndex+1, c => !c.IsOuter(leftCandle));
				if (midIndex == -1)
					continue;

				var midCandle = candles[midIndex];

				if (isMinimum && midCandle.Low > leftCandle.Low || !isMinimum && midCandle.High < leftCandle.High)
					continue;

				var rightIndex = candles.FindIndex(midIndex+1, c => !c.IsInner(midCandle));
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

		private List<Extremum> FindSecondExtremums(List<Extremum> firstExtremums, bool isMinimum)
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

		private static int GetStopResult(IEnumerable<Candle> dealCandles, bool isTrendLong, int stopLoss)
		{
			const int breakevenSize = 50;

			var candles = dealCandles as IList<Candle> ?? dealCandles.ToList();
			int startPrice = candles.First().Open;

			foreach (var candle in candles)
			{
				if (isTrendLong && candle.Low <= startPrice - stopLoss)
					return startPrice - stopLoss;

				if (!isTrendLong && candle.High >= startPrice + stopLoss)
					return startPrice + stopLoss;

				if (isTrendLong && candle.High >= startPrice + stopLoss ||
					!isTrendLong && candle.Low <= startPrice - stopLoss)
					stopLoss = -breakevenSize;
			}

			return -1;
		}
	}
}


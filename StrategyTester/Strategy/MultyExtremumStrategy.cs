using System;
using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;
using StrategyTester.Utils;

namespace StrategyTester.Strategy
{
	public class MultyExtremumStrategy
	{
		private readonly int stopLoss;
		private readonly int pegTopSize;
		private readonly int minExtremumStep;

		public MultyExtremumStrategy(int stopLoss, int pegTopSize, int minExtremumStep)
		{
			this.stopLoss = stopLoss;
			this.pegTopSize = pegTopSize;
			this.minExtremumStep = minExtremumStep;
		}

		public TradesResult Run(List<Day> days)
		{
			var result = new TradesResult();

			foreach (var day in days)
			{
				var profits = GetDaysDeal(day.FiveMins);
				if (profits == null)
					continue;

				foreach (var profit in profits)
				{
					result.AddDeal(profit);
				}
			}

			return result;
		}

		private bool IsPegTop(Candle candle)
		{
			return Math.Abs(candle.Open - candle.Close) <= pegTopSize;
		}

		private List<Deal> GetDaysDeal(List<Candle> daysCandles)
		{
			var allExtremums = FindAllSecondExtremums(daysCandles);
			bool inDeal = false;
			bool isTrendLong = false;
			int startIndex = 0;

			var deals = new List<Deal>();
			Extremum startExtremum = null;

			foreach (var extremum in allExtremums)
			{
				if (inDeal)
				{
					if (extremum.IsMinimum == isTrendLong)
						continue;

					if (isTrendLong && (extremum.Value - startExtremum.Value) < minExtremumStep ||
						!isTrendLong && (startExtremum.Value - extremum.Value) < minExtremumStep)
						continue;

					var startPrice = daysCandles[startIndex + 1].Open;
					var dynamicStopLoss = GetDynamicStopLoss(startPrice, isTrendLong, extremum);	//TODO исправить экстремум
					var dealCandles = daysCandles.Skip(startIndex + 1).Take(extremum.CheckerIndex - startIndex).ToList();

					var stopResult = GetStopResult(dealCandles, isTrendLong, dynamicStopLoss);
					var endPrice = stopResult != -1 ? stopResult : daysCandles[extremum.CheckerIndex].Close;
					deals.Add(new Deal(startPrice, endPrice, isTrendLong, daysCandles[startIndex + 1].DateTime, startIndex+1));

					inDeal = false;
					if (stopResult != -1)
						break;
				}

				startExtremum = extremum;
				startIndex = extremum.CheckerIndex;
				if (startIndex == daysCandles.Count - 1)
					break;

				isTrendLong = extremum.IsMinimum;
				inDeal = true;
			}

			if (inDeal)
			{
				var startPrice = daysCandles[startIndex + 1].Open;
				var dynamicStopLoss = GetDynamicStopLoss(startPrice, isTrendLong, null);
				var dealCandles = daysCandles.Skip(startIndex + 1).ToList();
				var stopResult = GetStopResult(dealCandles, isTrendLong, dynamicStopLoss);
				var endPrice = stopResult != -1 ? stopResult : daysCandles[dealCandles.Count - 1].Close;
				deals.Add(new Deal(startPrice, endPrice, isTrendLong, daysCandles[startIndex + 1].DateTime, startIndex+1));
			}
			 return deals;
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

			var secondLongExtremums = FindSecondExtremums(firstLongExtremums, true);
			var secondShortExtremums = FindSecondExtremums(firstShortExtremums, false);
			
			var allSecondExtremums = secondLongExtremums
				.Concat(secondShortExtremums)
				.OrderBy(ex => ex.CheckerIndex)
				.ToList();

			for (int i = 0; i < allSecondExtremums.Count - 1; ++i)
			{
				if (allSecondExtremums[i].Date >= allSecondExtremums[i + 1].Date)
				{
					allSecondExtremums.RemoveAt(i+1);
					--i;
				}
			}

			return allSecondExtremums;
		}

		private List<Extremum> FindFirstExtremums(List<Candle> candles, bool isMinimum)
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

		private static int GetStopResult(List<Candle> dealCandles, bool isTrendLong, int stopLoss)
		{
			const int breakevenSize = 50;

			int startPrice = dealCandles.First().Open;

			foreach (var candle in dealCandles)
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


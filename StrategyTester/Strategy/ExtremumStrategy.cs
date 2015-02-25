using System;
using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;
using StrategyTester.Utils;

namespace StrategyTester.Strategy
{
	class ExtremumStrategy
	{
		private class Extremum
		{
			public readonly DateTime Date;
			public int Value { get; private set; }
			public int CheckerIndex { get; private set; }
			public bool IsMinimum { get; private set; }

			public Extremum(int value, int checkerIndex, DateTime date, bool isMinimum)
			{
				Date = date;
				Value = value;
				CheckerIndex = checkerIndex;
				IsMinimum = isMinimum;
			}
		}

		private const int spread = 30;

		public TradesResult Run(List<Day> days, int stopLoss)
		{
			var result = new TradesResult();

			foreach (var day in days)
			{
				var profit = GetDaysDeal(day.FiveMins, stopLoss);
				if (profit == null)
					continue;

				result.AddDeal(profit);
			}

			return result;
		}

		private Deal GetDaysDeal(List<Candle> daysCandles, int stopLoss)
		{
			var firstLongExtremums = FindFirstExtremums(daysCandles, true);
			var firstShortExtremums = FindFirstExtremums(daysCandles, false);

			var secondExtremums =
				FindSecondExtremums(firstLongExtremums, true)
					.Concat(FindSecondExtremums(firstShortExtremums, false))
					.OrderBy(ex => ex.Date)
					.ToList();

			foreach (var extremum in secondExtremums)
			{
				var startIndex = extremum.CheckerIndex;
				var startPrice = daysCandles[startIndex + 1].Open;

				bool isTrendLong = startPrice > daysCandles.First().Open;
				if (isTrendLong != extremum.IsMinimum)
					continue;

				var stopResult = GetStopResult(daysCandles.Skip(startIndex + 1), isTrendLong, stopLoss);

				var endPrice = stopResult != -1 ? stopResult : daysCandles[daysCandles.Count - 1].Close;

				return new Deal(startPrice, endPrice, isTrendLong);
			}
			return null;
		}

		private Deal GetDaysDeal0(List<Candle> daysCandles, int stopLoss)
		{
			var firstLongExtremums = FindFirstExtremums(daysCandles, true);
			var firstShortExtremums = FindFirstExtremums(daysCandles, false);

			var secondLongExtremums = FindSecondExtremums(firstLongExtremums, true);
			var secondShortExtremums = FindSecondExtremums(firstShortExtremums, false);

			while (secondLongExtremums.Any() && secondShortExtremums.Any())
			{
				var longIndex = secondLongExtremums.First().CheckerIndex;
				var shortIndex = secondShortExtremums.First().CheckerIndex;

				var startIndex = Math.Min(longIndex, shortIndex);
				var startPrice = daysCandles[startIndex + 1].Open;

				bool isTrendLong = startPrice > daysCandles.First().Open;
				if (isTrendLong && (startIndex == shortIndex))
				{
					secondShortExtremums = secondShortExtremums.Skip(1).ToList();
					continue;
				}

				if (!isTrendLong && (startIndex == longIndex))
				{
					secondLongExtremums = secondLongExtremums.Skip(1).ToList();
					continue;
				}

				var stopResult = GetStopResult(daysCandles.Skip(startIndex+1), isTrendLong, stopLoss);
				var endPrice = stopResult != -1 ? stopResult : daysCandles[daysCandles.Count - 1].Close;

				return new Deal(startPrice, endPrice, isTrendLong);
			}
			return null;
		}

		private static List<Extremum> FindFirstExtremums(List<Candle> candles, bool isMinimum)
		{
			var exremums = new List<Extremum>();
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

				exremums.Add(new Extremum(isMinimum ? midCandle.Low : midCandle.High, rightIndex, midCandle.Date + midCandle.Time, isMinimum));
			}

			return exremums;
		}

		private static List<Extremum> FindSecondExtremums(List<Extremum> firstExtremums, bool isMinimum)
		{
			var exremums = new List<Extremum>();
			for (int i = 1; i < firstExtremums.Count - 1; ++i)
			{
				var currentValue = firstExtremums[i].Value;
				if (isMinimum && currentValue < firstExtremums[i - 1].Value &&
				    currentValue < firstExtremums[i + 1].Value ||
				    !isMinimum && currentValue > firstExtremums[i - 1].Value &&
				    currentValue > firstExtremums[i + 1].Value)
				{
					exremums.Add(new Extremum(currentValue, firstExtremums[i + 1].CheckerIndex, firstExtremums[i].Date, isMinimum));	
				}
			}

			return exremums;
		}

		private static int GetStopResult(IEnumerable<Candle> dealCandles, bool isTrendLong, int stopLoss)
		{
			var candles = dealCandles as IList<Candle> ?? dealCandles.ToList();
			int startPrice = candles.First().Open;

			if (isTrendLong && candles.Any(c => c.Low <= startPrice - stopLoss + spread))
				return startPrice - stopLoss;

			if (!isTrendLong && candles.Any(c => c.High >= startPrice + stopLoss - spread))
				return startPrice + stopLoss;

			return -1;
		}

		#region Bad

		private static bool IsTrendLong(List<Day> days, Func<Candle, int> getTrendSource)
		{
			var trendSource = getTrendSource(days[days.Count - 1].Params);
			var average = days.Average(day => getTrendSource(day.Params));
			return (trendSource > average);
			//return days[days.Count - 1].Params.Close > days.First().Params.Open;
		}

		public TradesResult Run(List<Day> days, int stopLoss, int averageCount)
		{
			var result = new TradesResult();
			var averageSource = days.Take(averageCount).ToList();
			days = days.Skip(averageCount).ToList();

			foreach (var day in days)
			{
				bool isTrendLong = IsTrendLong(averageSource, d => d.Close);
				averageSource.Add(day);
				averageSource.RemoveAt(0);

				var profit = GetDaysDeal(day.FiveMins, isTrendLong, stopLoss);
				if (profit == null)
					continue;

				result.AddDeal(profit);
			}

			return result;
		}

		private Deal GetDaysDeal(List<Candle> daysCandles, bool isTrendLong, int stopLoss)
		{
			var firstExtremums = FindFirstExtremums(daysCandles, isTrendLong);
			var secondExtremums = FindSecondExtremums(firstExtremums, isTrendLong);
			int result = 0;

			if (!secondExtremums.Any())
				return null;

			var startIndex = secondExtremums.First().CheckerIndex;
			var startCandle = daysCandles[startIndex];

			var startPrice = startCandle.Close;

			if (isTrendLong && daysCandles.Skip(startIndex).Any(c => c.Low <= startPrice - stopLoss + spread) ||
				!isTrendLong && daysCandles.Skip(startIndex).Any(c => c.High >= startPrice + stopLoss - spread))
				return new Deal(-stopLoss, isTrendLong);

			result = isTrendLong
				? daysCandles[daysCandles.Count - 1].Close - startPrice
				: startPrice - daysCandles[daysCandles.Count - 1].Close;

			return new Deal(result, isTrendLong);	//TODO Какой тренд сюда писать?
		}
#endregion

#region Unused
		private List<int> GetDaysDealsExtrOut(List<Candle> daysCandles, bool isTrendLong, int stopLoss)
		{
			var firstTrendExtremums = FindFirstExtremums(daysCandles, isTrendLong);
			var secondTrendExtremums = FindSecondExtremums(firstTrendExtremums, isTrendLong);

			var firstUntrendExtremums = FindFirstExtremums(daysCandles, !isTrendLong);
			var secondUntrendExtremums = FindSecondExtremums(firstUntrendExtremums, !isTrendLong);

			var result = new List<int>();

			while (secondTrendExtremums.Any())
			{
				int currentResult = 0;

				var startIndex = secondTrendExtremums.First().CheckerIndex;
				var startCandle = daysCandles[startIndex];
				var startPrice = startCandle.Close;

				secondUntrendExtremums = secondUntrendExtremums.SkipWhile(ex => ex.CheckerIndex <= startIndex).ToList();

				var endExtremum = secondUntrendExtremums.FirstOrDefault();
				int endIndex = endExtremum == null ? daysCandles.Count - 1 : endExtremum.CheckerIndex;
				var endCandle = daysCandles[endIndex];

				if (isTrendLong &&
				    daysCandles.Skip(startIndex).Take(endIndex - startIndex).Any(c => c.Low <= startPrice - stopLoss + spread) ||
				    !isTrendLong &&
				    daysCandles.Skip(startIndex).Take(endIndex - startIndex).Any(c => c.High >= startPrice + stopLoss - spread))
				{
					currentResult = -stopLoss;
					result.Add(currentResult);
					break;
				}
				
				currentResult = isTrendLong
					? endCandle.Close - startPrice
					: startPrice - endCandle.Close;

				secondTrendExtremums = secondTrendExtremums.SkipWhile(ex => ex.CheckerIndex <= endIndex).ToList();

				result.Add(currentResult);
				//break;
			}

			return result;
		}
		
		private List<int> GetDaysDeals(List<Candle> daysCandles, bool isTrendLong, int stopLoss)
		{
			var firstExtremums = FindFirstExtremums(daysCandles, isTrendLong);
			var secondExtremums = FindSecondExtremums(firstExtremums, isTrendLong);
			var result = new List<int>();

			while (secondExtremums.Any())
			{
				var startIndex = secondExtremums.First().CheckerIndex;
				var startCandle = daysCandles[startIndex];

				var startPrice = startCandle.Close;

				int index = isTrendLong
					? daysCandles.FindIndex(startIndex, c => c.Low <= startPrice - stopLoss + spread)
					: daysCandles.FindIndex(startIndex, c => c.High >= startPrice + stopLoss - spread);

				if (index != -1)
				{
					result.Add(-stopLoss);
					secondExtremums = secondExtremums.SkipWhile(ex => ex.CheckerIndex <= index).ToList();
					continue;
				}

				int currentResult = isTrendLong
					? daysCandles[daysCandles.Count - 1].Close - startPrice
					: startPrice - daysCandles[daysCandles.Count - 1].Close;

				result.Add(currentResult);
				break;
			}

			return result;
		}

#endregion
	}
}


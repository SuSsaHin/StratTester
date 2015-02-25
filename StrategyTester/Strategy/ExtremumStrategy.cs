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
			public DateTime Date;
			public int Value { get; private set; }
			public int CheckerIndex { get; private set; }

			public Extremum(int value, int checkerIndex, DateTime date)
			{
				Date = date;
				Value = value;
				CheckerIndex = checkerIndex;
			}
		}

		private const int spread = 30;

		public List<string> SExtremums = new List<string>(); 
			
		private static bool IsTrendLong(List<Day> days, Func<Candle, int> getTrendSource)
		{
			var trendSource = getTrendSource(days[days.Count - 1].Params);
			var average = days.Average(day => getTrendSource(day.Params));
			return (trendSource > average);
			//return days[days.Count - 1].Params.Close > days.First().Params.Open;
		}

		private static List<Extremum> FindFirstExtremumsBad(List<Candle> candles, bool isMinimum)
		{
			var exremums = new List<Extremum>();
			for (int leftIndex = 1; leftIndex < candles.Count - 2; ++leftIndex)
			{
				if (!isMinimum && candles[leftIndex - 1].High < candles[leftIndex].High && candles[leftIndex + 1].High < candles[leftIndex].High)
				{
					if (candles[leftIndex - 1].Low < candles[leftIndex].Low && candles[leftIndex + 1].Low < candles[leftIndex].Low)
					{
						exremums.Add(new Extremum(candles[leftIndex].High, leftIndex + 1, candles[leftIndex].Date + candles[leftIndex].Time));
					}
					else if (candles[leftIndex - 1].Low < candles[leftIndex].Low && candles[leftIndex + 2].Low < candles[leftIndex].Low && candles[leftIndex + 2].High < candles[leftIndex].High)
					{
						exremums.Add(new Extremum(candles[leftIndex].High, leftIndex + 2, candles[leftIndex].Date + candles[leftIndex].Time));
					}
				}
				else if (isMinimum && candles[leftIndex - 1].Low > candles[leftIndex].Low && candles[leftIndex + 1].Low > candles[leftIndex].Low)
				{
					if (candles[leftIndex - 1].High > candles[leftIndex].High && candles[leftIndex + 1].High > candles[leftIndex].High)
					{
						exremums.Add(new Extremum(candles[leftIndex].Low, leftIndex + 1, candles[leftIndex].Date + candles[leftIndex].Time));
					}
					else if (candles[leftIndex - 1].High > candles[leftIndex].High && candles[leftIndex + 2].High > candles[leftIndex].High && candles[leftIndex + 2].Low > candles[leftIndex].Low)
					{
						exremums.Add(new Extremum(candles[leftIndex].Low, leftIndex + 2, candles[leftIndex].Date + candles[leftIndex].Time));
					}
				}
			}

			return exremums;
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

				exremums.Add(new Extremum(isMinimum ? midCandle.Low : midCandle.High, rightIndex, midCandle.Date + midCandle.Time));
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
					exremums.Add(new Extremum(currentValue, firstExtremums[i + 1].CheckerIndex, firstExtremums[i].Date));	
				}
			}

			return exremums;
		}

		public TradesResult Run(List<Day> days, int stopLoss, int averageCount)
		{
			var result = new TradesResult();
			var averageSource = days.Take(averageCount).ToList();
			days = days.Skip(averageCount).ToList();

			//int successCount = 0, longCount = 0, shortCount = 0;

			foreach (var day in days)
			{
				bool isTrendLong = IsTrendLong(averageSource, d => d.Close);
				averageSource.Add(day);
				averageSource.RemoveAt(0);

				/*if (day.Params.IsLong && isTrendLong)
				{
					longCount++;
				}
				else if (!day.Params.IsLong && !isTrendLong)
				{
					shortCount++;
				}*/


				var profit = GetDaysDeal(day.FiveMins, isTrendLong, stopLoss);
				if (profit == 0)
					continue;

				result.AddDeal(profit);
				/*var profits = GetDaysDeals(day.FiveMins, isTrendLong, stopLoss);

				foreach (var profit in profits)
				{
					result.AddDeal(profit);	
				}*/
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

		private int GetDaysDeal(List<Candle> daysCandles, bool isTrendLong, int stopLoss)
		{
			var firstExtremums = FindFirstExtremums(daysCandles, isTrendLong);
			var secondExtremums = FindSecondExtremums(firstExtremums, isTrendLong);
			int result = 0;

			if (!secondExtremums.Any())
				return 0;

			//SExtremums.AddRange(secondExtremums.Select(ex => ex.Date.ToString() + " " + ex.Value.ToString()));

			var startIndex = secondExtremums.First().CheckerIndex;
			var startCandle = daysCandles[startIndex];

			var startPrice = startCandle.Close;

			if (isTrendLong && daysCandles.Skip(startIndex).Any(c => c.Low <= startPrice - stopLoss + spread) ||
				!isTrendLong && daysCandles.Skip(startIndex).Any(c => c.High >= startPrice + stopLoss - spread))
				return -stopLoss;

			result = isTrendLong
				? daysCandles[daysCandles.Count - 1].Close - startPrice
				: startPrice - daysCandles[daysCandles.Count - 1].Close;

			return result;
		}

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
	}
}


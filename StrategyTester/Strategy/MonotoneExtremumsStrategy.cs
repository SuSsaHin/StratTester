using System;
using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester.Strategy
{
	public class MonotoneExtremumsStrategy
	{
		private readonly ExtremumsFinder finder;
		private readonly int monotoneCount;
		private readonly int invertCount;
		private readonly int baseStopLoss;

		public MonotoneExtremumsStrategy(int monotoneCount, int baseStopLoss, int invertCount)
		{
			this.monotoneCount = monotoneCount;
			this.baseStopLoss = baseStopLoss;
			this.invertCount = invertCount;
			finder = new ExtremumsFinder(0);
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

		private Deal GetDaysDeal(List<Candle> daysCandles)
		{
			var longExtremums = finder.FindFirstExtremums(daysCandles, true);
			var shortExtremums = finder.FindFirstExtremums(daysCandles, false);

			var allExtremums = finder.MergeExtremums(longExtremums, shortExtremums);

			foreach (var extremum in allExtremums)
			{
				var startIndex = extremum.CheckerIndex;
				if (startIndex == daysCandles.Count - 1)
					break;

				var currentLong = longExtremums.TakeWhile(ex => ex.Date <= extremum.Date).ToList();
				var currentShort = shortExtremums.TakeWhile(ex => ex.Date <= extremum.Date).ToList();

				int trendDirection = GetTrendDirection(currentLong, currentShort, extremum);
				if (trendDirection == 0)
					continue;

				bool isTrendLong = trendDirection == 1;

				var startPrice = daysCandles[startIndex + 1].Open;
				var stopResult = GetStopResult(daysCandles.Skip(startIndex + 1), isTrendLong);

				var endPrice = stopResult != -1 ? stopResult : daysCandles[daysCandles.Count - 1].Close;

				return new Deal(startPrice, endPrice, isTrendLong);
			}
			return null;
		}

		private int GetTrendDirection(List<Extremum> currentLong, List<Extremum> currentShort, Extremum extremum)
		{
			var isTrendLong = extremum.IsMinimum;

			var trendExtremums = isTrendLong ? currentLong : currentShort;
			var unTrendExtremums = isTrendLong ? currentShort : currentLong;

			if (!AreMonotone(trendExtremums, monotoneCount, isTrendLong))
				return 0;

			if (AreMonotone(unTrendExtremums, invertCount, !isTrendLong))
				return 0;

			return isTrendLong ? 1 : -1;
		}

		private static bool AreMonotone(List<Extremum> extremums, int monotoneCount, bool isTrendLong)
		{
			if (extremums.Count < monotoneCount)
				return false;

			for (int i = extremums.Count - 1; i >= extremums.Count - monotoneCount + 1; --i)
			{
				if ((isTrendLong && extremums[i].Value <= extremums[i - 1].Value) ||
					(!isTrendLong && extremums[i].Value >= extremums[i - 1].Value))
					return false;
			}

			return true;
		}

		private int GetStopResult(IEnumerable<Candle> dealCandles, bool isTrendLong)
		{
			var candles = dealCandles as IList<Candle> ?? dealCandles.ToList();
			int startPrice = candles.First().Open;
			int stopLoss = baseStopLoss;

			foreach (var candle in candles)
			{
				if (isTrendLong && candle.Low <= startPrice - stopLoss)
					return startPrice - stopLoss;

				if (!isTrendLong && candle.High >= startPrice + stopLoss)
					return startPrice + stopLoss;
			}

			return -1;
		}
	}
}

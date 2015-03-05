using System;
using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester.Strategy
{
	public class MonotoneExtremumsStrategy
	{
		private readonly int monotoneCount;
		private readonly int invertCount;
		private readonly TimeSpan lastTradeTime;

		private readonly ExtremumsFinder finder;
		private readonly StopLossManager stopLossManager;

		public MonotoneExtremumsStrategy(int monotoneCount, int baseStopLoss, int invertCount, double breakevenPercent, TimeSpan? lastTradeTime = null)
		{
			this.monotoneCount = monotoneCount;
			this.invertCount = invertCount;
			this.lastTradeTime = lastTradeTime ?? new TimeSpan(23, 59, 59);

			finder = new ExtremumsFinder(0);
			stopLossManager = new StopLossManager(baseStopLoss, breakevenPercent, 1);
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
				if (extremum.Date.TimeOfDay > lastTradeTime)
					break;

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
				var stopResult = stopLossManager.GetBreakevenStopResult(daysCandles.Skip(startIndex + 1), isTrendLong);

				var endPrice = stopResult != -1 ? stopResult : daysCandles[daysCandles.Count - 1].Close;

				return new Deal(startPrice, endPrice, isTrendLong);
			}
			return null;
		}

		private int GetTrendDirection(List<Extremum> currentLong, List<Extremum> currentShort, Extremum extremum)
		{
			var isTrendLong = !extremum.IsMinimum;

			var trendExtremums = isTrendLong ? currentLong : currentShort;
			var unTrendExtremums = isTrendLong ? currentShort : currentLong;

			if (!AreMonotone(trendExtremums, monotoneCount, isTrendLong))
				return 0;

			if (unTrendExtremums.Count > 1)
			{

				if (unTrendExtremums.Count > invertCount)
				{
					unTrendExtremums = unTrendExtremums.Skip(unTrendExtremums.Count - invertCount).ToList();
				}
				var avg = unTrendExtremums.Average(ex => ex.Value - unTrendExtremums.First().Value);

				if (isTrendLong && avg < 0 || !isTrendLong && avg > 0)
					return 0;
			}
			/*if (AreMonotone(unTrendExtremums, invertCount, !isTrendLong))
				return 0;*/

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
	}
}

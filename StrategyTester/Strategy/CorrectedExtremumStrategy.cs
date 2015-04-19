using System;
using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester.Strategy
{
	public class CorrectedExtremumStrategy
	{
		private readonly ExtremumsFinder extremumsFinder;
		private readonly StopLossManager stopLossManager;
	    private readonly int maxDistFromOpen;

		public CorrectedExtremumStrategy(int baseStopLoss, int pegTopSize, double breakevenPercent, int trailingStopSize, int maxDistFromOpen = int.MaxValue)
		{
			extremumsFinder = new ExtremumsFinder(pegTopSize);
			stopLossManager = new StopLossManager(baseStopLoss, trailingStopSize, breakevenPercent);
		    this.maxDistFromOpen = maxDistFromOpen;
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
			var allExtremums = extremumsFinder.FindAllSecondExtremums(daysCandles);

			foreach (var extremum in allExtremums)
			{
				var startIndex = extremum.CheckerIndex;
				if (startIndex == daysCandles.Count - 1)
					break;

				var startCandle = daysCandles[startIndex + 1];
				var startPrice = startCandle.Open;

			    var distFromOpen = startPrice - daysCandles.First().Open;
			    bool isTrendLong = distFromOpen > 0;
				if (isTrendLong != extremum.IsMinimum)
					continue;

                if (Math.Abs(distFromOpen) > maxDistFromOpen)
                    return null;

                var stopResult = stopLossManager.GetTrailingStopResult(daysCandles.Skip(startIndex + 1), isTrendLong);

				var endPrice = stopResult != -1 ? stopResult : daysCandles[daysCandles.Count - 1].Close;

				return new Deal(startPrice, endPrice, isTrendLong);
			}
			return null;
		}
	}
}


using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester.Strategy
{
	public class CorrectedExtremumStrategy
	{
		private readonly ExtremumsFinder extremumsFinder;
		private readonly StopLossManager stopLossManager;

		public CorrectedExtremumStrategy(int baseStopLoss, int pegTopSize, double breakevenPercent)
		{
			extremumsFinder = new ExtremumsFinder(pegTopSize);
			stopLossManager = new StopLossManager(baseStopLoss, breakevenPercent);
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

				bool isTrendLong = startPrice > daysCandles.First().Open;
				if (isTrendLong != extremum.IsMinimum)
					continue;

				var stopResult = stopLossManager.GetBreakevenStopResult(daysCandles.Skip(startIndex + 1), isTrendLong);

				var endPrice = stopResult != -1 ? stopResult : daysCandles[daysCandles.Count - 1].Close;

				return new Deal(startPrice, endPrice, isTrendLong);
			}
			return null;
		}
	}
}


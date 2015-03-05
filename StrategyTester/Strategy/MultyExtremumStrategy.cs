using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester.Strategy
{
	public class MultyExtremumStrategy
	{
		private readonly int minExtremumStep;

		private readonly ExtremumsFinder extremumsFinder;
		private readonly StopLossManager stopLossManager;

		public MultyExtremumStrategy(int stopLoss, int pegTopSize, int minExtremumStep)
		{
			this.minExtremumStep = minExtremumStep;

			extremumsFinder = new ExtremumsFinder(pegTopSize);
			stopLossManager = new StopLossManager(stopLoss, 0.15);
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

		private List<Deal> GetDaysDeal(List<Candle> daysCandles)
		{
			var allExtremums = extremumsFinder.FindAllSecondExtremums(daysCandles);
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
					var dealCandles = daysCandles.Skip(startIndex + 1).Take(extremum.CheckerIndex - startIndex).ToList();

					var stopResult = stopLossManager.GetBreakevenStopResult(dealCandles, isTrendLong);
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

				var dealCandles = daysCandles.Skip(startIndex + 1).ToList();
				var stopResult = stopLossManager.GetBreakevenStopResult(dealCandles, isTrendLong);
				var endPrice = stopResult != -1 ? stopResult : daysCandles[dealCandles.Count - 1].Close;
				deals.Add(new Deal(startPrice, endPrice, isTrendLong, daysCandles[startIndex + 1].DateTime, startIndex+1));
			}
			 return deals;
		}
	}
}


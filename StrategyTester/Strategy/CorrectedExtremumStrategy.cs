using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester.Strategy
{
	public class CorrectedExtremumStrategy
	{
		private readonly int baseStopLoss;
		private readonly double breakevenPercent;
		private readonly ExtremumsFinder extremumsFinder;

		public CorrectedExtremumStrategy(int baseStopLoss, int pegTopSize, double breakevenPercent)
		{
			this.baseStopLoss = baseStopLoss;
			this.breakevenPercent = breakevenPercent;

			extremumsFinder = new ExtremumsFinder(pegTopSize);
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

				var stopResult = GetStopResult(daysCandles.Skip(startIndex + 1), isTrendLong);

				var endPrice = stopResult != -1 ? stopResult : daysCandles[daysCandles.Count - 1].Close;

				return new Deal(startPrice, endPrice, isTrendLong);
			}
			return null;
		}

		private int GetStopResult(IEnumerable<Candle> dealCandles, bool isTrendLong)
		{
			int stopLoss = baseStopLoss;
			var breakevenSize = (int)(breakevenPercent * stopLoss);

			var candles = dealCandles as IList<Candle> ?? dealCandles.ToList();
			int startPrice = candles.First().Open;

			foreach (var candle in candles)
			{
				if (isTrendLong && candle.Low <= startPrice - stopLoss)
					return startPrice - stopLoss;

				if (!isTrendLong && candle.High >= startPrice + stopLoss)
					return startPrice + stopLoss;

				if (stopLoss > -breakevenSize &&
					(isTrendLong && candle.High >= startPrice + stopLoss ||
					!isTrendLong && candle.Low <= startPrice - stopLoss))
					stopLoss = -breakevenSize;
			}

			return -1;
		}
	}
}


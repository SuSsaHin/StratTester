using System;
using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester.Strategy
{
	public class ExtremumStrategy
	{
		private readonly int baseStopLoss;
		private readonly double breakevenPercent;
		private readonly TimeSpan lastTradeTime;
		private readonly ExtremumsFinder extremumsFinder;

		public ExtremumStrategy(int baseStopLoss, int pegTopSize, double breakevenPercent, TimeSpan? lastTradeTime = null)
		{
			this.baseStopLoss = baseStopLoss;
			this.breakevenPercent = breakevenPercent;
			this.lastTradeTime = lastTradeTime ?? new TimeSpan(23, 59, 59);

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
				if (startIndex == daysCandles.Count-1)
					break;

				var startCandle = daysCandles[startIndex + 1];
				if (startCandle.Time > lastTradeTime)
					break;

				var startPrice = startCandle.Open;

//				bool isTrendLong = extremum.Value > daysCandles.First().Open;
				bool isTrendLong = startPrice > daysCandles.First().Open;

				if (isTrendLong != extremum.IsMinimum)
					continue;
				//isTrendLong = extremum.IsMinimum;

				/*if (!isTrendLong)
					continue;*/

				var dynamicStopLoss = GetDynamicStopLoss(startPrice, isTrendLong, extremum);

				var stopResult = GetStopResult(daysCandles.Skip(startIndex + 1), isTrendLong, dynamicStopLoss);

				var endPrice = stopResult != -1 ? stopResult : daysCandles[daysCandles.Count - 1].Close;

				return new Deal(startPrice, endPrice, isTrendLong);
			}
			return null;
		}

		private int GetDynamicStopLoss(int startPrice, bool isTrendLong, Extremum extremum)
		{
			/*const double maxStopRatio = 1.5;
			var dist = -GetProfit(isTrendLong, startPrice, extremum.Value);

			if (dist > stopLoss && dist < stopLoss*maxStopRatio)
				return dist;*/

			return baseStopLoss;
		}

		private int GetStopResult(IEnumerable<Candle> dealCandles, bool isTrendLong, int stopLoss)
		{
			//const int breakevenSize = 50;
			int breakevenSize = (int) (0.15*stopLoss);

			var candles = dealCandles as IList<Candle> ?? dealCandles.ToList();
			int startPrice = candles.First().Open;
			bool wasNew = false;
			
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

				var newStopLoss = (isTrendLong ? startPrice - candle.High : candle.Low - startPrice) + (int)(baseStopLoss*breakevenPercent);
				newStopLoss = (newStopLoss / 10) * 10;
				if (newStopLoss > 0 || newStopLoss > stopLoss)
					continue;

				stopLoss = newStopLoss;
			}

			return -1;
		}
	}
}


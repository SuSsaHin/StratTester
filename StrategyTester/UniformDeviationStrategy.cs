using System;
using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester
{
	class UniformDeviationStrategy
	{
		public const int TimeFrame = 60 * 5;
		
		public static double GetMiddleDiviation(IEnumerable<Tick> ticks, uint timeOffset, int maxDeviation)
		{
			if (timeOffset > TimeFrame)
				throw new Exception("Bad time offset");

			int candlesCount = 0;
			var lastTime = new TimeSpan();
			long deviationsSum = 0;
			int startValue = 0;
			int upDeviation = 0, downDeviation = 0;
			bool candleStarts = false;

			foreach (var tick in ticks)
			{
				if (tick.Time > lastTime && (((int)tick.Time.TotalSeconds % TimeFrame) == timeOffset))
				{
					if (candleStarts)
					{
						deviationsSum += Math.Min(upDeviation, downDeviation);
						upDeviation = downDeviation = 0;
					}

					lastTime = tick.Time;
					startValue = tick.Value;
					candleStarts = true;
					candlesCount++;
				}

				if (!candleStarts)
					continue;

				var dev = tick.Value - startValue;
				if (dev > maxDeviation)
				{
					candleStarts = false;
					deviationsSum += Math.Min(upDeviation, downDeviation);
					upDeviation = downDeviation = 0;
					continue;
				}

				if (dev > 0)
				{
					downDeviation = Math.Max(dev, downDeviation);
				}
				else if (dev < 0)
				{
					upDeviation = Math.Max(-dev, upDeviation);
				}
			}
			return deviationsSum/(double) candlesCount;
		}

		public static TradesResult TestTakeProfit(List<List<Tick>> candles, uint timeOffset, int exitMovingSize, int stopLoss, int takeProfit)
		{
			if (timeOffset > TimeFrame)
				throw new Exception("Bad time offset");

			var tradesResult = new TradesResult();

			foreach (var candle in candles)
			{
				int dealResult = GetCandleResult(candle, timeOffset, exitMovingSize, stopLoss, takeProfit);

				if (dealResult == -1)
					continue;

				tradesResult.AddDeal(dealResult);
			}

			return tradesResult;
		}

		public static int GetCandleResult(List<Tick> candle, uint timeOffset, int exitMovingSize, int stopLoss, int takeProfit)
		{
			bool inDeal = false;
			int dealStartValue = int.MaxValue;

			foreach (var tick in candle)
			{
				if (inDeal)
				{
					int div = tick.Value - dealStartValue;
					if (div < 0 && Math.Abs(div) > stopLoss ||
						div > takeProfit)
						return div;

					continue;
				}

				if (tick.Time.TotalSeconds%TimeFrame < timeOffset)
				{
					if (Math.Abs(tick.Value - candle.First().Value) > exitMovingSize)
						return -1;	//1 пункта для RTS не бывает

					continue;
				}

				dealStartValue = tick.Value;
				inDeal = true;
			}

			return candle.Last().Value - candle.First().Value;
		}
	}
}

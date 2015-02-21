using System;
using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester
{
	class ExtremumStrategy
	{
		private const int averageCount = 10;
			
		private static bool IsTrendLong(List<Day> days, Func<Candle, int> getTrendSource)
		{
			return getTrendSource(days[days.Count - 1].Params) > days.Average(day => getTrendSource(day.Params));
		}

		public TradesResult Run(List<Day> days, int stopLoss)
		{
			var result = new TradesResult();
			var averageSource = new List<Day>();

			for(int i = 1; i < days.Count; ++i)
			{
				averageSource.Add(days[i-1]);
				if (averageSource.Count > averageCount)
				{
					averageSource.RemoveAt(0);
				}
				bool isTrendLong = IsTrendLong(averageSource, d => d.Close);
				var profit = GetDaysDeal(days[i].FiveMins, isTrendLong, stopLoss);
				if (profit == 0)
					continue;

				result.AddDeal(profit);
			}

			return result;
		}

		private int GetDaysDeal(List<Candle> daysCandles, bool isTrendLong, int stopLoss)	//TODO вынести функцию FindExtremum(isMinimum)
		{
			var extInds = new int[3];
			int lastExtremum = 0;
			var leftCandle = daysCandles.First();
			int result = 0;

			for (int i = 1; i < daysCandles.Count; ++i)
			{
				var midCandle = daysCandles[i];
				if (midCandle.IsOuter(leftCandle))
					continue;

				if (isTrendLong && midCandle.High > leftCandle.High || !isTrendLong && midCandle.Low < leftCandle.Low)
				{
					leftCandle = midCandle;
					continue;
				}

				var rightIndex = daysCandles.FindIndex(i, c => !c.IsInner(midCandle));
				var rightCandle = daysCandles[rightIndex];

				if (isTrendLong && midCandle.High > rightCandle.High || !isTrendLong && midCandle.Low < rightCandle.Low)
				{
					leftCandle = midCandle;
					continue;
				}

				extInds[lastExtremum++] = i;

				if (lastExtremum < 3)
				{
					leftCandle = midCandle;
					continue;
				}

				if (isTrendLong 
					&& (daysCandles[extInds[1]].High < daysCandles[extInds[0]].High) 
					&& (daysCandles[extInds[1]].High < daysCandles[extInds[2]].High)) //TODO ВХОДИТЬ ПО daysCandles[extInds[1]].Hight
				{
					if (daysCandles.Skip(i).Any(c => c.Low <= rightCandle.Close - stopLoss))
					{
						result = -stopLoss;
					}
					else
					{
						result = (daysCandles[daysCandles.Count - 1].Close - rightCandle.Close);
					}

					break;
				}

				if (!isTrendLong
					&& (daysCandles[extInds[1]].Low > daysCandles[extInds[0]].Low)
					&& (daysCandles[extInds[1]].Low > daysCandles[extInds[2]].Low)) //TODO
				{
					if (daysCandles.Skip(i).Any(c => c.High >= rightCandle.Close + stopLoss))
					{
						result = -stopLoss;
					}
					else
					{
						result = (rightCandle.Close - daysCandles[daysCandles.Count - 1].Close);
					}
					
					break;
				}

				extInds[0] = extInds[1];
				extInds[1] = extInds[2];
				--lastExtremum;
				leftCandle = midCandle;
			}

			return result;
		}
	}
}

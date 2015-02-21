using System;
using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester
{
	class ProbabilityAnalyzer
	{
		public static Tuple<int, int> TestTrendInvertion(List<Day> days, int length, int skippedCount)
		{
			int samelyCount = 0, invertedCount = 0;
			for (int i = 1; i < days.Count; ++i)
			{
				var current = TestTrendCandlesInvertion(length, days[i - 1].Params.IsLong, days[i].FiveMins.Skip(skippedCount));
				samelyCount += current.Item1;
				invertedCount += current.Item2;
			}

			return new Tuple<int, int>(samelyCount, invertedCount);
		}

		private static Tuple<int, int> TestTrendCandlesInvertion0(int length, bool lastDayLong, IEnumerable<Candle> candles)
		{
			int invertedCount = 0, samelyCount = 0;

			var candlesList = candles as IList<Candle> ?? candles.ToList();
			int startValue = candlesList.First().Open;

			int currentCount = 1;
			bool needLong = candlesList.First().IsLong;
			//foreach (var candle in candlesList)
			for (int i = 1; i < candlesList.Count; ++i)
			{
				var candle = candlesList[i];
				bool currentDayLong = candle.Open > startValue;
				//if (!lastDayLong && currentDayLong || lastDayLong && !currentDayLong)
				//	continue;

				if (currentCount == length)
				{
					if (candle.IsLong == needLong)
					{
						samelyCount++;
						while (i < candlesList.Count && candlesList[i].IsLong == needLong)
						{
							++i;
						}
					}
					else
					{
						invertedCount++;
					}
					needLong = !needLong;
					currentCount = 1;
					continue;
				}
				if (candle.IsLong == needLong)
				{
					currentCount++;
				}
				else
				{
					currentCount = 1;
				}
			}

			return new Tuple<int, int>(samelyCount, invertedCount);
		}

		private static Tuple<int, int> TestTrendCandlesInvertion2(int countBeforeInvertion, bool lastDayLong, List<Candle> dayCandles)
		{
			int invertedCount = 0, samelyCount = 0;
			//int startValue = dayCandles.First().Open;
			int i = 1;
			while (i < dayCandles.Count)
			{
				int currentCount = 0;
				while (i < dayCandles.Count && dayCandles[i].IsLong == dayCandles[i-1].IsLong)
				{
					++i;
					++currentCount;
				}

				if (currentCount >= countBeforeInvertion)
				{
					++samelyCount;
				}
				else
				{
					++invertedCount;
				}
			}

			return new Tuple<int, int>(samelyCount, invertedCount);
		}

		private static Tuple<int, int> TestTrendCandlesInvertion(int length, bool lastDayLong, IEnumerable<Candle> oneDayCandles)
		{
			int invertedCount = 0, samelyCount = 0;

			var candlesList = oneDayCandles as IList<Candle> ?? oneDayCandles.ToList();
			int startValue = candlesList.First().Open;

			int currentCount = 0;
			bool needLong = candlesList.First().IsLong;

			for (int i = 1; i < candlesList.Count; ++i)
			{
				var candle = candlesList[i];

				bool currentDayLong = candle.Open > startValue;
				//if (!lastDayLong && currentDayLong || lastDayLong && !currentDayLong)
				//	continue;

				if (candle.IsLong == needLong)
				{
					currentCount++;
				}
				else
				{
					currentCount = 1;
					needLong = !needLong;
				}

				if (candlesList[i - 1].IsLong != currentDayLong)
					continue;

				if (currentCount == length)
				{
					if (candle.IsLong == candlesList[i-1].IsLong)
					{
						samelyCount++;
						while (i < candlesList.Count && candlesList[i].IsLong == candlesList[i - 1].IsLong)
						{
							++i;
						}
					}
					else
					{
						invertedCount++;
					}

					currentCount = 0;
				}
			}

			return new Tuple<int, int>(samelyCount, invertedCount);
		}

		public static Tuple<int, int> TestCandlesInvertion(List<Day> days, int length)
		{
			int samelyCount = 0, invertedCount = 0;
			foreach (var day in days)
			{
				var current = TestCandlesInvertion2(length, day.FiveMins);
				samelyCount += current.Item1;
				invertedCount += current.Item2;
			}

			return new Tuple<int, int>(samelyCount, invertedCount);
		}

		private static Tuple<int, int> TestCandlesInvertion(int length, IEnumerable<Candle> candles)
		{
			int invertedCount = 0, samelyCount = 0;

			var candlesList = candles as IList<Candle> ?? candles.ToList();

			int currentCount = 1;
			bool needLong = candlesList.First().IsLong;

			for (int i = 1; i < candlesList.Count; ++i)
			{
				var candle = candlesList[i];

				if (currentCount == length)
				{
					if (candle.IsLong == needLong)
					{
						samelyCount++;
						while (i < candlesList.Count && candlesList[i].IsLong == needLong)
						{
							++i;
						}
					}
					else
					{
						invertedCount++;
					}
					needLong = !needLong;
					currentCount = 1;
					continue;
				}
				if (candle.IsLong == needLong)
				{
					currentCount++;
				}
				else
				{
					currentCount = 1;
					needLong = !needLong;
				}
			}

			return new Tuple<int, int>(samelyCount, invertedCount);
		}

		private static Tuple<int, int> TestCandlesInvertion2(int countBeforeInvertion, List<Candle> dayCandles)
		{
			int invertedCount = 0, samelyCount = 0;
			int i = 1;

			while (i < dayCandles.Count)
			{
				int currentCount = 1;
				while (i < dayCandles.Count && dayCandles[i].IsLong == dayCandles[i - 1].IsLong)
				{
					++i;
					++currentCount;
				}
				++i;

				if (currentCount > countBeforeInvertion)
				{
					++samelyCount;
				}
				else if (currentCount == countBeforeInvertion)
				{
					++invertedCount;
				}
			}

			return new Tuple<int, int>(samelyCount, invertedCount);
		}
	}
}

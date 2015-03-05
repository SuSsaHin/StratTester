using System;
using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester
{
	public class ProbabilityAnalyzer
	{
		public static double TestTrendFromNCandle(List<Day> days, int candleNumber)
		{
			double successCount = 0;
			foreach (var day in days)
			{
				if (Math.Sign(day.FiveMins[candleNumber].Close - day.Params.Open) == Math.Sign(day.Params.Close - day.FiveMins[candleNumber].Close))
				{
					++successCount;
				}
			}
			return successCount/days.Count;
		}

		public static Tuple<int, int> TestExtremumsContinuation(List<Candle> candles, int monotoneCount, bool isMinimums)
		{
			var finder = new ExtremumsFinder(0);
			var extremums = finder.FindFirstExtremums(candles, isMinimums);

			int successCount = 0, failCount = 0;

			int currentMonotoneCount = 0;
			for (int i = 1; i < extremums.Count; ++i)
			{
				//var currentSign = Math.Sign(extremums[i].Value - extremums[i - 1].Value);
				bool currentTrend = extremums[i].Value > extremums[i - 1].Value;
				if (currentTrend == isMinimums)
				{
					currentMonotoneCount++;
					if (currentMonotoneCount >= monotoneCount)
					{
						successCount++;
					}
				}
				else
				{
					if (currentMonotoneCount >= monotoneCount)
					{
						failCount++;
					}
					currentMonotoneCount = 0;
				}
			}
			return new Tuple<int, int>(successCount, failCount);
		}

		public static List<int> TestExtremumsContinuationLength(List<Candle> candles, int monotoneCount, bool isMinimums)
		{
			var finder = new ExtremumsFinder(0);
			var extremums = finder.FindFirstExtremums(candles, isMinimums);

			var lengths = new List<int>();

			int currentMonotoneCount = 0;
			for (int i = 1; i < extremums.Count; ++i)
			{
				bool currentTrend = extremums[i].Value > extremums[i - 1].Value;
				if (currentTrend == isMinimums)
				{
					currentMonotoneCount++;
				}
				else
				{
					currentMonotoneCount = 0;
				}

				if (currentMonotoneCount >= monotoneCount)
				{
					lengths.Add(extremums[i].Value - extremums[i-1].Value);
				}
			}
			return lengths;
		}
	#region Old
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

	#endregion
	}
}

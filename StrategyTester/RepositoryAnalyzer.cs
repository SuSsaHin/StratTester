using System;
using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester
{
	class RepositoryAnalyzer
	{
		private HistoryRepository repository;

		public RepositoryAnalyzer(HistoryRepository repository)
		{
			this.repository = repository;
		}

		public Tuple<int, int> TestTrendInvertion(int length, int skippedCount)
		{
			int samelyCount = 0, invertedCount = 0;
			for (int i = 1; i < repository.Days.Count; ++i)
			{
				var current = TestTrendCandlesInvertion(length, repository.Days[i - 1].Params.IsLong, repository.Days[i].FiveMins.Skip(skippedCount));
				samelyCount += current.Item1;
				invertedCount += current.Item2;
			}

			return new Tuple<int, int>(samelyCount, invertedCount);
		}

		public Tuple<int, int> TestTrendInvertion(int length)
		{
			int samelyCount = 0, invertedCount = 0;
			for (int i = 1; i < repository.Days.Count; ++i)
			{
				var current = TestCandlesInvertion(length, repository.Days[i - 1].Params.IsLong, repository.Days[i].FiveMins);
				samelyCount += current.Item1;
				invertedCount += current.Item2;
			}

			return new Tuple<int, int>(samelyCount, invertedCount);
		}

		public Tuple<int, int> TestCandlesInvertion(int length, bool needLong)
		{
			return TestCandlesInvertion(length, needLong, repository.Candles);
		}

		private static Tuple<int, int> TestCandlesInvertion(int length, bool needLong, IEnumerable<Candle> candles)
		{
			int invertedCount = 0, samelyCount = 0;
			int currentCount = 0;
			foreach (var candle in candles)
			{
				if (currentCount == length)
				{
					if (candle.IsLong && needLong || !candle.IsLong && !needLong)
					{
						samelyCount++;
					}
					else
					{
						invertedCount++;
					}
					currentCount = 0;
				}
				if (candle.IsLong && needLong || !candle.IsLong && !needLong)
				{
					currentCount++;
				}
				else
				{
					currentCount = 0;
					;
				}
			}

			return new Tuple<int, int>(samelyCount, invertedCount);
		}

		private static Tuple<int, int> TestTrendCandlesInvertion(int length, bool lastDayLong, IEnumerable<Candle> candles)
		{
			int invertedCount = 0, samelyCount = 0;

			var candlesList = candles as IList<Candle> ?? candles.ToList();
			//int startValue = candlesList.First().Open;

			int currentCount = 1;
			bool needLong = candlesList.First().IsLong;
			//foreach (var candle in candlesList)
			for (int i = 1; i < candlesList.Count; ++i)
			{
				var candle = candlesList[i];
				//bool currentDayLong = candle.Open > startValue;
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
	}
}

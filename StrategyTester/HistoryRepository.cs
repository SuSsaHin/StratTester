using System;
using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester
{
	class HistoryRepository
	{
		public List<Day> Days { get; private set; }

		public IEnumerable<Candle> Candles 
		{
			get { return Days.SelectMany(day => day.FiveMins); }
		} 
		
		private static List<Candle> GetCandles(IEnumerable<Tick> ticks, int periodMins)
		{
			return ticks.GroupBy(t => t.Date).SelectMany(day => day.GroupBy(t => (int)t.Time.TotalMinutes / periodMins).Select(frame => new Candle(frame.ToList(), periodMins))).ToList();
		}

		public HistoryRepository(string toolName, bool isTicks)
		{
			if (isTicks)
			{
				FillByTicks(toolName);
				return;
			}

			ReadCandles(toolName);
		}

		private void ReadCandles(string toolName)
		{
			Days = HistoryReader.ReadCandles(toolName, 5);
		}

		private void FillByTicks(string toolName)
		{
			Days = new List<Day>();

			var files = HistoryReader.GetTicksFiles(toolName);

			if (!files.Any())
				throw new Exception("Empty history");

			foreach (var filename in files)
			{
				var ticks = HistoryReader.ReadTicks(filename);
				var dayCandle = GetCandles(ticks, 60*24).Single();
				var fiveMins = GetCandles(ticks, 5);

				Days.Add(new Day(dayCandle, fiveMins));
			}

			Days = Days.OrderBy(day => day.Params.Date).Distinct().ToList();
		}
	}
}

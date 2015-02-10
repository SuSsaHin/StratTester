using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester
{
	class HistoryRepository
	{
		private const string dataPath = @"..\..\History\";
		public const string ToolName = @"SBER-3.15";
		
		public List<Day> Days { get; private set; }

		public IEnumerable<Candle> Candles 
		{
			get { return Days.SelectMany(day => day.FiveMins); }
		} 
		
		private static List<Candle> GetCandles(IEnumerable<Tick> ticks, int periodMins)
		{
			return ticks.GroupBy(t => t.Date).SelectMany(day => day.GroupBy(t => (int)t.Time.TotalMinutes / periodMins).Select(frame => new Candle(frame.ToList(), periodMins))).ToList();
		}

		public HistoryRepository()
		{
			FillDays();
		}

		private void FillDays()
		{
			Days = new List<Day>();

			var path = Path.Combine(dataPath, ToolName);
			var files = Directory.GetFiles(path, "*.txt");

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

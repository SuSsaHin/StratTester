using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester
{
	class HistoryReader
	{
		private const string dataPath = @"..\..\History\";
		private const string ticksDirectory = @"Ticks";
		private const string candlesDirectory = @"Candles";
		private const string filesExtension = ".txt";
		
		private static DateTime ParseDateTime(string dateStr, string timeStr)
		{
			var time = int.Parse(timeStr);
			var date = int.Parse(dateStr);

			var second = time % 100;
			time /= 100;
			var minute = time % 100;
			var hour = time / 100;

			var day = date % 100;
			date /= 100;
			var month = date % 100;
			var year = date / 100;

			return new DateTime(year, month, day, hour, minute, second);
		}

		private static Tick ParseTick(string row)
		{
			var fields = row.Split('\t');

			var dateTime = ParseDateTime(fields[0], fields[1]);

			var nextTick = new Tick(dateTime, (int)decimal.Parse(fields[2], new CultureInfo("en-us")));
			return nextTick;
		}

		private static Candle ParseCandle(string row)
		{
			var fields = row.Split('\t');
			var candle = new Candle(ParseDateTime(fields[0], fields[1]),
									(int)decimal.Parse(fields[2], new CultureInfo("en-us")),
									(int)decimal.Parse(fields[3], new CultureInfo("en-us")),
									(int)decimal.Parse(fields[4], new CultureInfo("en-us")),
									(int)decimal.Parse(fields[5], new CultureInfo("en-us")),
									5);
			return candle;
		}

		public static string[] GetTicksFiles(string toolName)
		{
			var path = Path.Combine(dataPath, ticksDirectory, toolName);
			var files = Directory.GetFiles(path, "*" + filesExtension);

			if (!files.Any())
				throw new Exception("Empty history");

			return files;
		}

		public static List<Tick> ReadTicks(string filename)
		{
			var readed = File.ReadAllLines(filename);
			var result = new List<Tick>();
			var lastTick = new Tick(new DateTime(), 0);

			foreach (var row in readed)
			{
				var nextTick = ParseTick(row);

				if (nextTick.Equals(lastTick))
					continue;

				lastTick = nextTick;
				result.Add(nextTick);
			}

			return result;
		}

		public static List<Day> ReadCandles(string toolName, int periodMins, uint startTime = 0, uint endTime = int.MaxValue)
		{
			var filename = Path.Combine(dataPath, candlesDirectory, toolName) + filesExtension;

			var readed = File.ReadAllLines(filename);
			var result = new List<Day>();
			var lastDate = new DateTime();
			var candles = new List<Candle>();

			foreach (var row in readed)
			{
				var candle = ParseCandle(row);

				if (!candles.Any())
				{
					lastDate = candle.Date;
				}

				if (lastDate.Date != candle.Date)
				{
					result.Add(new Day(candles));
					candles = new List<Candle>();
					lastDate = candle.Date;
				}
				candles.Add(candle);
			}
			result.Add(new Day(candles));

			return result;
		}

		public static void OptimizeFile(string path)
		{
			var lines = File.ReadAllLines(path);
			lines = lines.Distinct().ToArray();
			File.WriteAllLines(path, lines);
		}
	}
}

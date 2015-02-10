using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester
{
	class HistoryReader
	{
		public static List<Tick> ReadTicks(string filename, uint startTime = 0, uint endTime = int.MaxValue)
		{
			var readed = File.ReadAllLines(filename);
			var result = new List<Tick>();
			var lastTick = new Tick(new DateTime(), 0);

			foreach (var row in readed)
			{
				var fields = row.Split('\t', '.');

				var time = int.Parse(fields[1]);
				if (time < startTime || time > endTime)
					continue;

				var second = time % 100;
				time /= 100;
				var minute = time%100;
				var hour = time / 100;

				var date = int.Parse(fields[0]);
				var day = date%100;
				date /= 100;
				var month = date%100;
				var year = date / 100;

				var dateTime = new DateTime(year, month, day, hour, minute, second);

				var nextTick = new Tick(dateTime, int.Parse(fields[2]));

				if (nextTick.Equals(lastTick))
					continue;

				lastTick = nextTick;
				result.Add(nextTick);
			}

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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace StrategyTester
{
	public class TrendPredictorTester
	{
		public static string TestFuzzy(string historyFilename, string predictedFilename, int skipCount)
		{
			var history = ReadArray(historyFilename);
			var predicted = ReadArray(predictedFilename);

			skipCount++;

			//File.WriteAllLines("predicted.txt", predicted.Select(d => d.ToString()));

			//int skipCount = predicted.TakeWhile(d => Math.Abs(d) < 0.00001).Count();
			predicted = predicted.Skip(skipCount).ToList();
			history = history.Skip(skipCount).ToList();

			int successCounter = 0;

			for (int i = 1; i < history.Count; ++i)
			{
				if (Math.Sign(history[i] - history[i - 1]) == Math.Sign(predicted[i] - history[i - 1]))
				{
					successCounter++;
				}
			}

			return successCounter + @" / " + history.Count;
		}

		public static int TestAverage(string historyFilename, int averageCount, int shortAverageCount)
		{
			var history = ReadArray(historyFilename);

			var averageSource = history.Take(averageCount).ToList();
			history = history.Skip(averageCount - 1).ToList();

			int successCounter = 0;

			for (int i = 1; i < history.Count; ++i)
			{
				bool isTrendLong = IsTrendLong(averageSource, shortAverageCount);
				averageSource.Add(history[i]);
				averageSource.RemoveAt(0);

				if ((history[i] - history[i - 1] > 0) == isTrendLong)
				{
					successCounter++;
				}
			}

			return successCounter;
		}

		private static bool IsTrendLong(List<double> days, int shortAverageCount)
		{
			var shortAverage = days.Skip(days.Count - shortAverageCount).Average();
			var average = days.Average();
			return (shortAverage > average);
			//return days[days.Count - 1] > days.First();
		}

		private static List<double> ReadArray(string filename)
		{
			return File.ReadAllLines(filename).Select(str => Double.Parse(str, new CultureInfo("en-us"))).ToList();
		}
	}
}

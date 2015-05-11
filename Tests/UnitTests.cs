using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using StrategyTester;
using Assert = NUnit.Framework.Assert;

namespace Tests
{
	[TestClass]
	class UnitTests
	{
		[TestCase("RTS-14", 70, true)]
		[TestCase("RTS-14", 70, false)]
		[TestCase("RTS-15", 70, true)]
		[TestCase("RTS-15", 70, false)]
		public static void TestFirstExtremumsSortByDate(string toolName, int pegTopSize, bool isMinimums)
		{
			var repository = new HistoryRepository(toolName, false);
			var extremumFinder = new ExtremumsFinder(pegTopSize);
			
			bool bad = false;
			foreach (var day in repository.Days)
			{
				var extremums = extremumFinder.FindFirstExtremums(day.FiveMins, isMinimums);
				
				for (int i = 1; i < extremums.Count; ++i)
				{
						if (extremums[i-1].Date >= extremums[i].Date)
						{
							Console.Out.WriteLine(extremums[i - 1] + "\t" + extremums[i]);
							bad = true;
						}
				}

				Assert.That(!bad);
			}
		}

		[TestCase("RTS-14", 70, true)]
		[TestCase("RTS-14", 70, false)]
		[TestCase("RTS-15", 70, true)]
		[TestCase("RTS-15", 70, false)]
		public static void TestFirstExtremumsSortByChecker(string toolName, int pegTopSize, bool isMinimums)
		{
			var repository = new HistoryRepository(toolName, false);
			var extremumFinder = new ExtremumsFinder(pegTopSize);

			bool bad = false;
			foreach (var day in repository.Days)
			{
				var extremums = extremumFinder.FindFirstExtremums(day.FiveMins, isMinimums);

				for (int i = 1; i < extremums.Count; ++i)
				{
					if (extremums[i - 1].CheckerIndex > extremums[i].CheckerIndex)
					{
						Console.Out.WriteLine(extremums[i - 1] + "\t" + extremums[i]);
						bad = true;
					}
				}

				Assert.That(!bad);
			}
		}

		[TestCase("RTS-14", 70, true)]
		[TestCase("RTS-14", 70, false)]
		[TestCase("RTS-15", 70, true)]
		[TestCase("RTS-15", 70, false)]
		public static void TestFirstExtremumsOrder(string toolName, int pegTopSize, bool isMinimums)
		{
			var repository = new HistoryRepository(toolName, false);

			var extremumFinder = new ExtremumsFinder(pegTopSize);
			foreach (var day in repository.Days)
			{
				var extremums = extremumFinder.FindFirstExtremums(day.FiveMins, isMinimums);

				bool bad = false;
				for (int i = 0; i < extremums.Count; ++i)	//TODO Люой экстремум 
				{
					for (int j = i+1; j < extremums.Count; ++j)
					{
						if (extremums[j].Date >= extremums[i].Date && extremums[j].CheckerIndex < extremums[i].CheckerIndex)
						{
							Console.Out.WriteLine(extremums[i] + "\t" + extremums[j]);
							bad = true;
						}
					}
				}

				Assert.That(!bad);
			}
		}

		[TestCase("RTS-14", 70, true)]
		[TestCase("RTS-14", 70, false)]
		[TestCase("RTS-15", 70, true)]
		[TestCase("RTS-15", 70, false)]
		public static void TestFirstExtremumsDoubles(string toolName, int pegTopSize, bool isMinimum)
		{
			var repository = new HistoryRepository(toolName, false);

			var extremumFinder = new ExtremumsFinder(pegTopSize);
			foreach (var day in repository.Days)
			{
				var extremums = extremumFinder.FindFirstExtremums(day.FiveMins, isMinimum);

				bool bad = false;
				for (int i = 0; i < extremums.Count; ++i)
				{
					for (int j = i + 1; j < extremums.Count; ++j)
					{
						if (extremums[i].Date == extremums[j].Date && extremums[i].CheckerIndex == extremums[j].CheckerIndex)
						{
							Console.Out.WriteLine(extremums[i] + "\t" + extremums[j]);
							bad = true;
						}
					}
				}
				Assert.That(!bad);
			}
		}

		[TestCase("RTS-14", 70)]
		[TestCase("RTS-15", 70)]
		public static void TestSecondExtremumsOrder(string toolName, int pegTopSize)
		{
			var repository = new HistoryRepository(toolName, false);

			var extremumFinder = new ExtremumsFinder(pegTopSize);
			foreach (var day in repository.Days)
			{
				var extremums = extremumFinder.FindAllSecondExtremums(day.FiveMins);

				bool bad = false;
				for (int i = 1; i < extremums.Count; ++i)
				{
					if (/*extremums[i].IsMinimum == extremums[i-1].IsMinimum && */extremums[i].Date >= extremums[i - 1].Date && extremums[i].CheckerIndex < extremums[i - 1].CheckerIndex)
					{
						Console.Out.WriteLine(extremums[i] + "\t" + extremums[i - 1]);
						bad = true;
					}
				}

				Assert.That(!bad);
			}
		}

//		[TestCase("RTS-14", 70)]
//		[TestCase("RTS-15", 70)]
//		public static void TestSecondExtremumsAlternation(string toolName, int pegTopSize)
//		{
//			var repository = new HistoryRepository(toolName, false);
//			var extremumFinder = new ExtremumsFinder(pegTopSize);
//
//			bool bad = false;
//			foreach (var day in repository.Days)
//			{
//				if (day.Params.Date != new DateTime(2015, 04, 17))
//				{
//					continue;
//				}
//				var firstMaximums = extremumFinder.FindFirstExtremums(day.FiveMins, false);
//				foreach (var maximum in firstMaximums)
//				{
//					Console.WriteLine(maximum);
//				}
//				Console.WriteLine();
//				var extremums = extremumFinder.FindAllSecondExtremums(day.FiveMins);
//				
//				
//				for (int i = 1; i < extremums.Count; ++i)
//				{
//					if (extremums[i].IsMinimum == extremums[i - 1].IsMinimum)
//					{
//						Console.Out.WriteLine(extremums[i] + "\t" + extremums[i - 1]);
//						bad = true;
//					}
//				}
//			}
//			Assert.That(!bad);
//		}

		[TestCase("RTS-14", 70)]
		[TestCase("RTS-15", 70)]
		public static void TestSecondExtremumsAlternation(string toolName, int pegTopSize)
		{
			//TODO ошибка в том, что реализм требует сортировки firstExtremums по checkerIndex, а текущий метод поиска secondExtremums - сортировки по Date. В общем случае, обоюдная сортировка невозможна
			var repository = new HistoryRepository(toolName, false);
			var extremumFinder = new ExtremumsFinder(pegTopSize);

			bool bad = false;
			foreach (var day in repository.Days)
			{
				var firstMaximums = extremumFinder.FindFirstExtremums(day.FiveMins, false);
				var maximums = extremumFinder.FindSecondExtremums(firstMaximums, false);

				var firstMinimums = extremumFinder.FindFirstExtremums(day.FiveMins, true);
				var minimums = extremumFinder.FindSecondExtremums(firstMinimums, true);

				var allExtremums = maximums
					.Concat(minimums)
					.OrderBy(ex => ex.CheckerIndex)
					.ToList();

				for (int i = 0; i < allExtremums.Count - 1; ++i)
				{
					if (allExtremums[i].Date >= allExtremums[i + 1].Date && allExtremums[i].IsMinimum == allExtremums[i + 1].IsMinimum)
					{
						bad = true;
						Console.Out.WriteLine(allExtremums[i] + "\t" + allExtremums[i + 1]);
					}
				}
			}
			Assert.That(!bad);
		}

	}
}

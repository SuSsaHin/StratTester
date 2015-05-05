using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using StrategyTester;
using Assert = NUnit.Framework.Assert;

namespace Tests
{
	[TestClass]
	class UnitTests
	{
		[TestCase("RTS-14",	70)]
		[TestCase("RTS-15",	70)]
        public static void TestFindFirstExtremums(string toolName, int pegTopSize)
		{
			var repository = new HistoryRepository(toolName, false);

		    var extremumFinder = new ExtremumsFinder(pegTopSize);
		    foreach (var day in repository.Days)
		    {
                var minimums = extremumFinder.FindFirstExtremums(day.FiveMins, true);

		        bool bad = false;
		        for (int i = 1; i < minimums.Count; ++i)
		        {
		            if (minimums[i].Date >= minimums[i - 1].Date && minimums[i].CheckerIndex < minimums[i - 1].CheckerIndex)
		            {
		                Console.Out.WriteLine(minimums[i] + "\t" + minimums[i-1]);
		                bad = true;
		            }
		        }

                var maximums = extremumFinder.FindFirstExtremums(day.FiveMins, false);
                for (int i = 1; i < maximums.Count; ++i)
                {
                    if (maximums[i].Date >= maximums[i - 1].Date && maximums[i].CheckerIndex < maximums[i - 1].CheckerIndex)
                    {
                        Console.Out.WriteLine(maximums[i] + "\t" + maximums[i - 1]);
                        bad = true;
                    }
                }

                Assert.That(!bad);
		    }
		}

        [TestCase("RTS-14", 70)]
        [TestCase("RTS-15", 70)]
        public static void TestFindAllSecondExtremums(string toolName, int pegTopSize)
        {
            var repository = new HistoryRepository(toolName, false);

            var extremumFinder = new ExtremumsFinder(pegTopSize);
            foreach (var day in repository.Days)
            {
                var extremums = extremumFinder.FindAllSecondExtremums(day.FiveMins);

                bool bad = false;
                for (int i = 1; i < extremums.Count; ++i)
                {
                    if (extremums[i].IsMinimum == extremums[i - 1].IsMinimum)
                    {
                        Console.Out.WriteLine(extremums[i] + "\t" + extremums[i-1]);
                        bad = true;
                    }
                }
                Assert.That(!bad);
            }
        }

	}
}

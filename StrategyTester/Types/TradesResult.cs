using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StrategyTester.Types
{
	class TradesResult
	{
		private const int comission = 10;

		private readonly List<int> deals = new List<int>();
		private int currentSag = 0;

		public int GoodCount { get; private set; }
		public int BadCount { get; private set; }
		public int Profit { get; private set; }
		public int Volume { get; private set; }
		public int MaxLoss { get; private set; }
		public int MaxProfit { get; private set; }
		public int MaxSag { get; private set; }

		public double ProfitAverage
		{
			get { return deals.Where(d => d > 0).Average(); }
		}

		public double LossAverage
		{
			get { return deals.Where(d => d < 0).Average(); }
		}

		public override string ToString()
		{
			return string.Format("Good: {0}, Bad: {1}, Profit: {2}, Volume: {3}, Profit percent: {4},\nMax loss: {5}, Max profit: {6}, Max sag: {7}, Profit average: {8}, Loss average: {9}", 
								GoodCount, BadCount, Profit, Volume, 100*Math.Round(Profit / (double) Volume, 3), MaxLoss, MaxProfit, MaxSag, Math.Round(ProfitAverage, 2), Math.Round(LossAverage, 2));
		}

		public void AddDeal(int dealResult)
		{
			dealResult -= comission;
			if (dealResult > 0)
			{
				GoodCount++;
				MaxProfit = Math.Max(MaxProfit, dealResult);
				if (currentSag > MaxSag)
				{
					MaxSag = currentSag;
				}
				currentSag = 0;
			}
			else
			{
				BadCount++;
				MaxLoss = Math.Max(MaxLoss, -dealResult);
				currentSag -= dealResult;
			}
			Profit += dealResult;
			Volume += Math.Abs(dealResult);

			deals.Add(dealResult);
		}

		public void PrintDeals()
		{
			for (int i = 0; i < deals.Count; ++i)
			{
				Console.WriteLine("{0}: {1}", i, deals[i]);
			}
		}

		public void PrintDeals(string filename)
		{
			File.WriteAllLines(filename, deals.ConvertAll(d => d.ToString()));
		}

		public void PrintDepo(string filename)
		{
			var depo = new List<int>{0};
			int sum = 0;
			foreach (var deal in deals)
			{
				sum += deal;
				depo.Add(sum);
			}
			File.WriteAllLines(filename, depo.ConvertAll(d => d.ToString()));
		}
	}
}

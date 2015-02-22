using System;
using System.Collections.Generic;
using System.IO;

namespace StrategyTester.Types
{
	class TradesResult
	{
		private List<int> deals = new List<int>();
		public int GoodCount;
		public int BadCount;
		public int Profit;
		public int Volume;
		public int MaxLoss;
		public int MaxProfit;

		public override string ToString()
		{
			return string.Format("Good: {0}, Bad: {1}, Profit: {2}, Volume: {3}, Profit percent: {4},\n Max loss: {5}, Max profit: {6}", 
								GoodCount, BadCount, Profit, Volume, 100*Math.Round(Profit / (double) Volume, 3), MaxLoss, MaxProfit);
		}

		public void AddDeal(int dealResult)
		{
			if (dealResult > 0)
			{
				GoodCount++;
				MaxProfit = Math.Max(MaxProfit, dealResult);
			}
			else
			{
				BadCount++;
				MaxLoss = Math.Max(MaxLoss, -dealResult);
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

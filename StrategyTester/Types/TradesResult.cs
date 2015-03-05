using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StrategyTester.Types
{
	public class TradesResult
	{
		private readonly List<Deal> deals = new List<Deal>();

		private int currentSag;
		private int currentSagLength;

		public int MaxDropdown { get; private set; }
		public int MaxDropdownLength { get; private set; }

		public int DealsCount { get { return deals.Count; } }

		public int GoodCount 
		{
			get { return deals.Count(d => d.IsGood); } 
		}

		public int BadCount
		{
			get { return deals.Count(d => !d.IsGood); }
		}

		public int Profit
		{
			get { return deals.Sum(d => d.Profit); }
		}

		public int Volume
		{
			get { return deals.Sum(deal => Math.Abs(deal.Profit)); }
		}

		public int MaxLoss
		{
			get
			{
				var badDeals = deals.Where(d => !d.IsGood).ToList();
				return badDeals.Any() ? Math.Abs(badDeals.Min(d => d.Profit)) : 0;
			}
		}

		public int MaxProfit
		{
			get
			{
				var goodDeals = deals.Where(d => d.IsGood).ToList();
				return goodDeals.Any() ? goodDeals.Max(d => d.Profit) : 0;
			}
		}

		public double ProfitAverage
		{
			get
			{
				var goodDeals = deals.Where(d => d.IsGood).ToList();
				return goodDeals.Any() ? goodDeals.Average(d => d.Profit) : 0;
			}
		}

		public double LossAverage
		{
			get
			{
				var badDeals = deals.Where(d => !d.IsGood).ToList();
				return badDeals.Any() ? badDeals.Average(d => d.Profit) : 0;
			}
		}

		public int LongGoodCount
		{
			get { return deals.Count(d => d.IsGood && d.IsTrendLong); }
		}

		public int ShortGoodCount
		{
			get { return deals.Count(d => d.IsGood && !d.IsTrendLong); }
		}

		public override string ToString()
		{
			return string.Format("Good: {0}, Bad: {1}, Profit: {2}, Volume: {3}, Profit percent: {4},\n" +
								 "Max loss: {5}, Max profit: {6}, Max dropdown: {7}, Max dropdown length: {8},\n" +
			                     "Profit average: {9}, Loss average: {10}, Long good: {11}, short good: {12}", 
								GoodCount, BadCount, Profit, Volume, 100*Math.Round(Profit / (double) Volume, 3), 
								MaxLoss, MaxProfit, MaxDropdown, MaxDropdownLength,
								Math.Round(ProfitAverage, 2), Math.Round(LossAverage, 2), LongGoodCount, ShortGoodCount);
		}

		public void AddDeal(Deal deal)
		{
			if (deal.IsGood)
			{
				MaxDropdown = Math.Max(MaxDropdown, currentSag);
				currentSag = 0;

				MaxDropdownLength = Math.Max(MaxDropdownLength, currentSagLength);
				currentSagLength = 0;
			}
			else
			{
				currentSag -= deal.Profit;
				currentSagLength++;
			}

			deals.Add(deal);
		}

		public void PrintDeals()
		{
			for (int i = 0; i < deals.Count; ++i)
			{
				Console.WriteLine("{0}: {1}", i, deals[i].Profit);
			}
		}

		public void PrintDeals(string filename)
		{
			File.WriteAllLines(filename, deals.ConvertAll(d => d.Profit.ToString()));
		}

		public void PrintDepo(string filename)
		{
			var depo = new List<int>{0};
			int sum = 0;
			foreach (var deal in deals)
			{
				sum += deal.Profit;
				depo.Add(sum);
			}
			File.WriteAllLines(filename, depo.ConvertAll(d => d.ToString()));
		}
	}
}

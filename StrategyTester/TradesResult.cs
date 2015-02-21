using System;

namespace StrategyTester
{
	struct TradesResult
	{
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
		}
	}
}

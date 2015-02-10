using System;

namespace StrategyTester
{
	struct TradesResult
	{
		public int GoodCount;
		public int BadCount;
		public int Profit;
		public int Volume;

		public override string ToString()
		{
			return string.Format("Good: {0}, Bad: {1}, Profit: {2}, Volume: {3}, Profit percent: {4}", GoodCount, BadCount, Profit, Volume, 100*Math.Round(Profit / (double) Volume, 3));
		}

		public void AddDeal(int dealResult)
		{
			if (dealResult > 0)
			{
				GoodCount++;
			}
			else
			{
				BadCount++;
			}
			Profit += dealResult;
			Volume += Math.Abs(dealResult);
		}
	}
}

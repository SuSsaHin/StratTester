namespace StrategyTester.Types
{
	class Deal
	{
		private const int comission = 10;

		public Deal(int profit, bool isTrendLong)
		{
			Profit = profit - comission;
			IsTrendLong = isTrendLong;
		}

		public Deal(int startPrice, int endPrice, bool isTrendLong)
		{
			Profit = isTrendLong ? endPrice - startPrice : startPrice - endPrice;
			Profit -= comission;
			IsTrendLong = isTrendLong;
		}

		public bool IsTrendLong { get; private set; }
		public int Profit { get; private set; }
		public bool IsGood { get { return Profit > 0; } }
	}
}

using System;

namespace StrategyTester.Types
{
	public class Deal
	{
		private const int comission = 30;

		public Deal(int profit, bool isTrendLong)
		{
			Profit = profit - comission;
			IsTrendLong = isTrendLong;
		}

		public Deal(int startPrice, int endPrice, bool isTrendLong):
			this(isTrendLong ? endPrice - startPrice : startPrice - endPrice, isTrendLong)
		{}

		public Deal(int startPrice, int endPrice, bool isTrendLong, DateTime start, int extremumIndex = 0):
			this(startPrice, endPrice, isTrendLong)
		{
			Start = start;
			ExtremumIndex = extremumIndex;
		}

		public bool IsTrendLong { get; private set; }
		public int Profit { get; private set; }
		public DateTime Start { get; private set; }
		public int ExtremumIndex { get; private set; }

		public bool IsGood { get { return Profit > 0; } }
	}
}

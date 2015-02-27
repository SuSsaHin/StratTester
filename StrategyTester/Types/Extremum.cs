using System;

namespace StrategyTester.Types
{
	class Extremum
	{
		public readonly DateTime Date;
		public int Value { get; private set; }
		public int CheckerIndex { get; private set; }
		public bool IsMinimum { get; private set; }

		public Extremum(int value, int checkerIndex, DateTime date, bool isMinimum)
		{
			Date = date;
			Value = value;
			CheckerIndex = checkerIndex;
			IsMinimum = isMinimum;
		}
	}
}

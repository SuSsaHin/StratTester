using System;

namespace StrategyTester.Types
{
    public class Extremum
	{
		public readonly DateTime Date;
		public int Value { get; private set; }
		public int CheckerIndex { get; private set; }
		public bool IsMinimum { get; private set; }

		public bool CanBeSecond { get; set; }

		public Extremum(int value, int checkerIndex, DateTime date, bool isMinimum)
		{
			Date = date;
			Value = value;
			CheckerIndex = checkerIndex;
			IsMinimum = isMinimum;
			CanBeSecond = true;
		}

        public override string ToString()
        {
            return Date + " (" + CheckerIndex +  "): " + Value + ", " + (IsMinimum ? "min" : "max") + ", " + (CanBeSecond ? "Can be second" : "Can't be second");
        }
	}
}

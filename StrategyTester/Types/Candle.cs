using System;
using System.Collections.Generic;
using System.Linq;

namespace StrategyTester.Types
{
	class Candle
	{
		public Candle(List<Tick> ticks, int periodMins)
		{
			PeriodMins = periodMins;
			Ticks = ticks;

			InitParams();
		}

		private void InitParams()
		{
			var first = Ticks.First();
			var last = Ticks.Last();
			//var period = (last.Time - first.Time);
			//if (period.Seconds != 0 || (int)period.TotalMinutes != PeriodMins)
			//	throw new Exception("Ticks are not coordinated with period");

			Open = first.Value;
			Close = last.Value;
			High = 0;
			Low = int.MaxValue;

			foreach (var tick in Ticks)
			{
				if (tick.Value > High)
				{
					High = tick.Value;
				}
				if (tick.Value < Low)
				{
					Low = tick.Value;
				}
			}
		}

		public List<Tick> Ticks { get; private set; }

		public int Open { get; private set; }
		public int Close { get; private set; }
		public int High { get; private set; }
		public int Low { get; private set; }

		public int PeriodMins { get; private set; }

		public bool IsLong
		{
			get { return Close > Open; }
		}

		public DateTime Date
		{
			get { return Ticks.First().Date; }
		}

		public TimeSpan Time
		{
			get { return Ticks.First().Time; }
		}
	}
}

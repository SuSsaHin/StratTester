using System;
using System.Collections.Generic;
using System.Linq;

namespace StrategyTester.Types
{
	public class Candle
	{
		public Candle(List<Tick> ticks, int periodMins)
		{
			PeriodMins = periodMins;
			Ticks = ticks;
			dateTime = ticks.First().Date + ticks.First().Time;

			InitParams();
		}

		public Candle(DateTime dateTime, int open, int hight, int low, int close, int periodMins)
		{
			Open = open;
			High = hight;
			Low = low;
			Close = close;

			/*if (hight < low || hight < open || hight < close ||
			    low > open || low > close)
			{
				int a = 0;
			}*/

			PeriodMins = periodMins;
			this.dateTime = dateTime;
			Ticks = new List<Tick>();
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

		private DateTime dateTime;

		public bool IsLong
		{
			get { return Close > Open; }
		}

		public DateTime Date
		{
			get { return dateTime.Date; }
		}

		public TimeSpan Time
		{
			get { return dateTime.TimeOfDay; }
		}

		public DateTime DateTime
		{
			get { return dateTime; }
		}

	    public int InnerHeigth
	    {
            get { return Math.Abs(Open - Close); }
	    }
	}
}

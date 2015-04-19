using System;
using System.Collections.Generic;
using System.Linq;

namespace StrategyTester.Types
{
	public class Day
	{
		public readonly Candle Params;
		public readonly List<Candle> FiveMins;

		public Day(Candle @params, List<Candle> fiveMins)
		{
			Params = @params;
			FiveMins = fiveMins;
		}

		public Day(List<Candle> fiveMins)
		{
			FiveMins = fiveMins;
			Params = new Candle(fiveMins.First().Date + fiveMins.First().Time,
								fiveMins.First().Open,
								fiveMins.Max(c => c.High),
								fiveMins.Min(c => c.Low),
								fiveMins[fiveMins.Count - 1].Close,
								24*60);
		}
	}
}

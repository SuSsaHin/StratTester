using System.Collections.Generic;

namespace StrategyTester.Types
{
	class Day
	{
		public readonly Candle Params;
		public readonly List<Candle> FiveMins;

		public Day(Candle @params, List<Candle> fiveMins)
		{
			Params = @params;
			FiveMins = fiveMins;
		}
	}
}

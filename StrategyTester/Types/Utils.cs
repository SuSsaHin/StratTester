using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyTester.Types
{
	static class Utils
	{
		public static bool IsInner(this Candle current, Candle previous)
		{
			return current.High < previous.High && current.Low < previous.Low;
		}

		public static bool IsOuter(this Candle current, Candle previous)
		{
			return current.High > previous.High && current.Low > previous.Low;
		}
	}
}

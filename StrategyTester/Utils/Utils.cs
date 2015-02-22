using StrategyTester.Types;

namespace StrategyTester.Utils
{
	static class Utils
	{
		public static bool IsInner(this Candle current, Candle previous)
		{
			return current.High < previous.High && current.Low > previous.Low;
		}

		public static bool IsOuter(this Candle current, Candle previous)
		{
			return current.High > previous.High && current.Low < previous.Low;
		}
	}
}

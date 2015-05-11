using System;
using System.Globalization;
using StrategyTester.Types;

namespace StrategyTester.Utils
{
	public static class Utils
	{
		public static bool IsInner(this Candle current, Candle previous)
		{
			return current.High < previous.High && current.Low > previous.Low;
		}

		public static bool IsOuter(this Candle current, Candle previous)
		{
			return current.High > previous.High && current.Low < previous.Low;
		}

		public static int Middle(this Candle candle)
		{
			return (candle.Close + candle.Open)/2;
		}

		public static string ToEnString(this double num)
		{
			return num.ToString(new CultureInfo("en-us"));
		}
	}
}

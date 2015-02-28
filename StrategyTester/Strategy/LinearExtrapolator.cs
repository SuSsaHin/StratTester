using System;
using System.Collections.Generic;
using StrategyTester.Types;
using StrategyTester.Utils;

namespace StrategyTester.Strategy
{
	class LinearExtrapolator
	{
		private double lineTan;

		private double GetLine(double x)
		{
			return lineTan*x;
		}

		public void Run(Day day)
		{
			var candles = day.FiveMins;

			var startIndex = GetStartIndex(candles);
			if (startIndex == -1)
				throw new Exception("Bad start index");

			lineTan = ((candles[startIndex + 1].Middle() + candles[startIndex + 2].Middle()) / 2.0 - candles[startIndex].Middle()) / 1.5;


		}

		private int GetStartIndex(List<Candle> candles)
		{
			for (int i = 1; i < candles.Count-2; ++i)
			{
				if ((candles[i].IsLong == candles[i + 1].IsLong) && (candles[i].IsLong == candles[i + 2].IsLong))
				{
					return i;
				}
			}
			return -1;
		}
	}
}

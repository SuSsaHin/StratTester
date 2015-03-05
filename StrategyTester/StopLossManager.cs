using System.Collections.Generic;
using System.Linq;
using StrategyTester.Types;

namespace StrategyTester
{
	class StopLossManager
	{
		private readonly int baseStopLoss;
		private readonly int breakevenSize;
		private readonly int trailingSize;
		private readonly int breakevenInitizlizerSize;

		public StopLossManager(int baseStopLoss, int trailingSize, double breakevenPercent = 0, double breakevenInitizlizerPercent = 1)
		{
			this.baseStopLoss = baseStopLoss;
			this.trailingSize = trailingSize;

			breakevenInitizlizerSize = (int)(breakevenInitizlizerPercent * baseStopLoss);
			breakevenSize = (int)(breakevenPercent * baseStopLoss);
		}

		public StopLossManager(int baseStopLoss, double breakevenPercent = 0, double breakevenInitizlizerPercent = 1) :
			this(baseStopLoss, baseStopLoss, breakevenPercent, breakevenInitizlizerPercent)
		{}

		public int GetStopResult(IEnumerable<Candle> dealCandles, bool isTrendLong)
		{
			var candles = dealCandles as IList<Candle> ?? dealCandles.ToList();
			int startPrice = candles.First().Open;

			foreach (var candle in candles)
			{
				if (isTrendLong && candle.Low <= startPrice - baseStopLoss)
					return startPrice - baseStopLoss;

				if (!isTrendLong && candle.High >= startPrice + baseStopLoss)
					return startPrice + baseStopLoss;
			}

			return -1;
		}

		public int GetBreakevenStopResult(IEnumerable<Candle> dealCandles, bool isTrendLong)
		{
			int stopLoss = baseStopLoss;

			var candles = dealCandles as IList<Candle> ?? dealCandles.ToList();
			int startPrice = candles.First().Open;

			foreach (var candle in candles)
			{
				if (isTrendLong && candle.Low <= startPrice - stopLoss)
					return startPrice - stopLoss;

				if (!isTrendLong && candle.High >= startPrice + stopLoss)
					return startPrice + stopLoss;

				if (stopLoss != -breakevenSize)
				{
					stopLoss = GetBreakeven(isTrendLong, candle, startPrice);
				}
			}

			return -1;
		}

		public int GetTrailingStopResult(IEnumerable<Candle> dealCandles, bool isTrendLong)
		{
			int stopLoss = baseStopLoss;

			var candles = dealCandles as IList<Candle> ?? dealCandles.ToList();
			int startPrice = candles.First().Open;

			foreach (var candle in candles)
			{
				if (isTrendLong && candle.Low <= startPrice - stopLoss)
					return startPrice - stopLoss;

				if (!isTrendLong && candle.High >= startPrice + stopLoss)
					return startPrice + stopLoss;

				if (stopLoss > -breakevenSize)
				{
					stopLoss = GetBreakeven(isTrendLong, candle, startPrice);
				}

				var newStopLoss = (isTrendLong ? startPrice - candle.High : candle.Low - startPrice) + trailingSize;
				if (newStopLoss > 0 || newStopLoss > stopLoss)
					continue;

				stopLoss = newStopLoss;
			}

			return -1;
		}

		public int GetDynamicStopLoss(int startPrice, bool isTrendLong, Extremum extremum)
		{
			const double maxStopRatio = 1.5;
			var dist = -(isTrendLong ? extremum.Value - startPrice : startPrice - extremum.Value);

			if (dist > baseStopLoss && dist < baseStopLoss * maxStopRatio)
				return dist;

			return baseStopLoss;
		}

		private int GetBreakeven(bool isTrendLong, Candle candle, int startPrice)
		{
			if ((isTrendLong && candle.High >= startPrice + breakevenInitizlizerSize ||
				 !isTrendLong && candle.Low <= startPrice - breakevenInitizlizerSize))
				return -breakevenSize;

			return baseStopLoss;
		}
	}
}

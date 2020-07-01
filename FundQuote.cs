using System;

namespace p2cprice
{
	public class FundQuote
	{
		public DateTime Date { get; internal set; }
		public string Name { get; internal set; }
		public string Ticker { get; internal set; }
		public double Price { get; internal set; }
		public double UsdChange { get; internal set; }
		public double PercentChange { get; internal set; }
	}
}

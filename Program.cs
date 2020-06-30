using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace p2cprice
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var url = "https://www.path2college529.com/research/daily.shtml";
			var outputFile = args.Length > 0 ? args[0] : @"C:\users\david\downloads\p2c.csv";

			_funds = new Dictionary<string, string>();
			_funds["Conservative Allocation Portfolio"] = "";
			_funds["Balanced Allocation Portfolio"] = "";
			_funds["High Equity Allocation Portfolio"] = "";
			_funds["100% Fixed-Income Portfolio"] = "";
			_funds["U.S. Equity Index Portfolio"] = "P2C8511";

			try
			{
				using (var writer = File.CreateText(outputFile))
					await downloadQuotes(url, writer);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.ToString());
			}
		}

		static async Task downloadQuotes(string url, TextWriter writer)
		{
			var web = new HtmlWeb();
			var doc = await web.LoadFromWebAsync(url);

			var staticInvestmentPortfolioNode = doc.DocumentNode.SelectSingleNode("//div[@class='panel-title' and text() = 'Static Investment Portfolios']");
			if (staticInvestmentPortfolioNode == null)
				return;

			var tableNode = staticInvestmentPortfolioNode.SelectSingleNode("../..//table");
			var dateNode = tableNode.SelectSingleNode("thead/tr/th[2]");
			var dateText = dateNode.InnerText;

			if (!DateTime.TryParseExact(dateText, "Uni\\t Value a\\s o\\f MMMM dd, yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime quoteDate))
				quoteDate = DateTime.Today;

			foreach (var rowNode in tableNode.SelectNodes("tbody/tr"))
			{
				var name = rowNode.SelectSingleNode("td[1]").InnerText;
				var ticker = _funds[name];
				if (string.IsNullOrEmpty(ticker))
					continue;

				var price = rowNode.SelectSingleNode("td[2]").InnerText.Replace("$", "");

				writer.WriteLine("{0},{1:MM/dd/yy},{2}", ticker, quoteDate, price);
			}
		}

		private static Dictionary<string, string> _funds = null;
	}
}

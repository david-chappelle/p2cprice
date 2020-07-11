using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;

namespace p2cprice
{
	public class QuoteDownloader
	{
		public string Url { get; set; }
		public Fund[] Funds { get; set; }

		public void LoadConfig(string configFile = null)
		{
			var cfg = new ConfigurationBuilder()
				.AddJsonFile(configFile ?? "config.json")
				.Build();

			Url = cfg["url"];
			Funds = cfg.GetSection("funds").Get<Fund[]>();
		}

		public async Task<FundQuote[]> GetTodaysQuotes()
		{
			var web = new HtmlWeb();
			var doc = await web.LoadFromWebAsync(Url);

			// look for the "Static Investment Portfolios" section
			var staticInvestmentPortfolioNode = doc.DocumentNode.SelectSingleNode("//div[@class='panel-title' and text() = 'Static Investment Portfolios']");
			if (staticInvestmentPortfolioNode == null)
				return null;

			// navigate up to the main table node
			var tableNode = staticInvestmentPortfolioNode.SelectSingleNode("../..//table");

			// locate and read the quote date
			var dateNode = tableNode.SelectSingleNode("thead/tr/th[2]");
			var dateText = dateNode.InnerText;
			if (!string.IsNullOrEmpty(dateText))
				dateText = dateText.Replace("Unit Value as of ", "");

			// if the date is parseable, parse it, otherwise use today's date
			var quoteDate = DateTime.MinValue;
			if (!DateTime.TryParseExact(dateText, "MMMM d, yyyy", null, System.Globalization.DateTimeStyles.None, out quoteDate))
				quoteDate = DateTime.Today;

			var quotes = new List<FundQuote>();

			// each fund is a row in the table
			foreach (var rowNode in tableNode.SelectNodes("tbody/tr"))
			{
				// col 0 = fund name
				// col 1 = price
				// col 2 = change (USD)
				// col 3 = change (%)
				var name = rowNode.SelectSingleNode("td[1]").InnerText;
				var fund = Funds.FirstOrDefault(f => f.name == name);
				if (fund == null)
				{
					// this fund is not in our json config file
					continue;
				}

				var fundQuote = new FundQuote();
				fundQuote.Name = fund.name;
				fundQuote.Ticker = fund.ticker;
				fundQuote.Date = quoteDate;

				var price = rowNode.SelectSingleNode("td[2]").InnerText.Replace("$", "");
				fundQuote.Price = double.Parse(price);

				var usdChangeNode = rowNode.SelectSingleNode("td[3]/div[2]") ?? rowNode.SelectSingleNode("td[3]/span[1]");
				if (usdChangeNode != null && double.TryParse(usdChangeNode.InnerText, out double usdChange))
					fundQuote.UsdChange = usdChange;

				var percentChangeNode = rowNode.SelectSingleNode("td[4]");
				if (percentChangeNode != null && double.TryParse(percentChangeNode.InnerText.Replace("%", ""), out double percentChange))
					fundQuote.PercentChange = percentChange;

				quotes.Add(fundQuote);
			}

			return quotes.ToArray();
		}
	}
}

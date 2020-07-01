using System;
using System.IO;
using System.Threading.Tasks;

namespace p2cprice
{
	class Program
	{
		static async Task<int> Main(string[] args)
		{
			try
			{
				var downloader = new QuoteDownloader();
				downloader.LoadConfig();

				var outputFile = args.Length > 0 ? args[0] : @"C:\users\david\downloads\p2c.csv";
				var quotes = await downloader.GetTodaysQuotes();

				using (var writer = File.CreateText(outputFile))
				{
					foreach (var quote in quotes)
					{
						// write to csv file
						writer.WriteLine("{0},{1:MM/dd/yy},{2:0.00}", quote.Ticker, quote.Date, quote.Price);
					}
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.ToString());
				return 1;
			}

			return 0;
		}
	}
}

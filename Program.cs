using System.Globalization;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;
using ServiceReference;

class Program
{
  private static readonly CultureInfo ruCulture = CultureInfo.GetCultureInfo("ru-RU");
  private static readonly KeyRateDataStore dataStore = new KeyRateDataStore();

  static void Main(string[] args)
  {
    // Show historical data on startup
    ShowHistoricalData();

    Timer stateTimer = new Timer((state) =>
    {
      CheckKeyRateAPI(state);
      CheckKeyRateRssFeed(state);
    }, null, 0, 1000 * 60 * 120); // It will check every two hours

    // Handle shutdown gracefully
    Console.CancelKeyPress += (sender, e) =>
    {
      e.Cancel = true;
      dataStore.Dispose();
      Environment.Exit(0);
    };

    while (true) { Thread.Sleep(1000); } // Prevent console app from closing
  }

  private static void CheckKeyRateAPI(object? stateInfo)
  {
    var client = new DailyInfoSoapClient(DailyInfoSoapClient.EndpointConfiguration.DailyInfoSoap12);
    var now = DateTime.Now;
    var response = client.KeyRateAsync(now.AddDays(-30), now).Result;
    var rates = new List<(DateTime date, decimal rate)>();
    
    foreach (var element in response.Nodes)
    {
      if (element.Name.LocalName == "diffgram")
      {
        foreach (var keyRate in element.Elements())
        {
          foreach (var kr in keyRate.Elements())
          {
            var date = DateTime.Now;
            var rate = 0.0M;

            foreach (var innerElement in kr.Elements())
            {
              if (innerElement.Name.LocalName == "DT")
              {
                DateTime.TryParse(innerElement.Value, out date);
              }
              if (innerElement.Name.LocalName == "Rate")
              {
                decimal.TryParse(innerElement.Value, out rate);
              }
            }
            rates.Add((date, rate));
          }
        }
      }
    }
    
    // Sort by date
    rates = rates.OrderBy(r => r.date).ToList();
    
    if (rates.Count > 0)
    {
      var lastRate = rates.Last().rate;
      var effectiveFromDate = rates.Where(r => r.rate == lastRate).First().date;
      Console.WriteLine($"API: Last key rate {lastRate} (effective from {effectiveFromDate.ToString("s")})");

      // Store each rate in the database
      foreach (var (date, rate) in rates)
      {
        dataStore.StoreKeyRate(date, rate, "API");
      }
    }
  }

  private static void CheckKeyRateRssFeed(object? stateInfo)
  {
    string url = "https://www.cbr.ru/rss/RssPress";
    using var reader = XmlReader.Create(url);
    var feed = SyndicationFeed.Load(reader);
    var lastDate = DateTime.MinValue;
    var lastRate = 0.0M;
    foreach (var item in feed.Items)
    {
      if (item.Title.Text.Contains("ключевую ставку"))
      {
        // Use regex to get the key rate value and date from the title
        var regex = new Regex(@"до\s+(?<rate>\d+(,\d+)?)%\s+годовых\s+\((?<date>\d{2}.\d{2}.\d{4})\)");
        var match = regex.Match(item.Title.Text);
        if (match.Success)
        {
          var rate = decimal.Parse(match.Groups["rate"].Value, ruCulture);
          var date = DateTime.Parse(match.Groups["date"].Value, ruCulture);
          if (date > lastDate)
          {
            lastDate = date;
            lastRate = rate;
          }
        }
      }
    }
    Console.WriteLine($"RSS: Last key rate {lastRate} (published at {lastDate.ToString("s")})");

    // Store the rate in the database if we found one
    if (lastRate > 0 && lastDate > DateTime.MinValue)
    {
      dataStore.StoreKeyRate(lastDate, lastRate, "RSS");
    }
  }

  private static void ShowHistoricalData()
  {
    Console.WriteLine("=== Historical Key Rates from Database ===");

    var historicalRates = dataStore.GetAllKeyRates();
    if (historicalRates.Count == 0)
    {
      Console.WriteLine("No historical data found in database.");
    }
    else
    {
      Console.WriteLine($"Found {historicalRates.Count} historical entries:");
      foreach (var (date, rate, source) in historicalRates.Take(10)) // Show last 10 entries
      {
        Console.WriteLine($"{date:s}: {rate}% ({source})");
      }

      if (historicalRates.Count > 10)
      {
        Console.WriteLine($"... and {historicalRates.Count - 10} more entries");
      }

      var latest = dataStore.GetLatestKeyRate();
      if (latest.HasValue)
      {
        Console.WriteLine($"Latest rate in database: {latest.Value.rate}% on {latest.Value.date:s}");
      }
    }
    Console.WriteLine("==========================================\n");
  }

  private static void ListAllKeyRates(object? stateInfo)
  {
    var client = new DailyInfoSoapClient(DailyInfoSoapClient.EndpointConfiguration.DailyInfoSoap12);
    var response = client.KeyRateAsync(new DateTime(2023, 08, 01), new DateTime(2023, 12, 19)).Result;

    foreach (var element in response.Nodes)
    {
      if (element.Name.LocalName == "diffgram")
      {
        foreach (var keyRate in element.Elements())
        {
          foreach (var kr in keyRate.Elements())
          {
            var date = DateTime.Now;
            var rate = 0.0M;

            foreach (var innerElement in kr.Elements())
            {
              if (innerElement.Name.LocalName == "DT")
              {
                DateTime.TryParse(innerElement.Value, out date);
              }
              if (innerElement.Name.LocalName == "Rate")
              {
                decimal.TryParse(innerElement.Value, out rate);
              }
            }
            Console.WriteLine($"{date.ToString("s")}: {rate}");
          }
        }
      }
    }
  }
}
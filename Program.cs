using System.Globalization;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;
using ServiceReference;
using Telegram.Bot;
using Telegram.Bot.Types;

class Program
{
  private static readonly CultureInfo ruCulture = CultureInfo.GetCultureInfo("ru-RU");
  private static TelegramBotClient? botClient;
  private static string? telegramChatId;
  private static decimal lastKnownRate = 0;
  private static DateTime lastKnownRateDate = DateTime.MinValue;

  static void Main(string[] args)
  {
    InitializeTelegramBot();

    Timer stateTimer = new Timer((state) =>
    {
      CheckKeyRateAPI(state);
      CheckKeyRateRssFeed(state);
    }, null, 0, 1000 * 60 * 120); // It will check every two hours
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

      // Check if rate changed and send Telegram notification
      if (lastKnownRate != lastRate && lastKnownRate != 0)
      {
        var message = $"🏦 CBR Key Rate Changed!\n\nNew rate: {lastRate}%\nEffective from: {effectiveFromDate:dd.MM.yyyy}\nPrevious rate: {lastKnownRate}%";
        _ = Task.Run(() => SendTelegramNotification(message));
      }

      lastKnownRate = lastRate;
      lastKnownRateDate = effectiveFromDate;
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

    // Send notification if we found a rate that's different from our known rate
    if (lastRate > 0 && lastKnownRate != lastRate && lastKnownRate != 0)
    {
      var message = $"📰 CBR Key Rate Update from RSS!\n\nNew rate: {lastRate}%\nPublished: {lastDate:dd.MM.yyyy}\nPrevious rate: {lastKnownRate}%";
      _ = Task.Run(() => SendTelegramNotification(message));
    }
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

  private static void InitializeTelegramBot()
  {
    var botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
    telegramChatId = Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID");

    if (!string.IsNullOrEmpty(botToken))
    {
      botClient = new TelegramBotClient(botToken);
      Console.WriteLine("Telegram bot initialized successfully");
    }
    else
    {
      Console.WriteLine("Warning: TELEGRAM_BOT_TOKEN not set. Telegram notifications disabled.");
    }
  }

  private static async Task SendTelegramNotification(string message)
  {
    if (botClient != null && !string.IsNullOrEmpty(telegramChatId))
    {
      try
      {
        await botClient.SendMessage(chatId: telegramChatId, text: message);
        Console.WriteLine($"Telegram notification sent: {message}");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Failed to send Telegram notification: {ex.Message}");
      }
    }
  }
}
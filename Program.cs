class Program
{
  static void Main(string[] args)
  {
    Timer stateTimer = new Timer(CheckKeyRate, null, 0, 1000 * 60 * 120); // It will check every two hours
    while (true) { Thread.Sleep(1000); } // Prevent console app from closing
  }

  private static void CheckKeyRate(object? stateInfo)
  {
    var client = new DailyInfoSoapClient(DailyInfoSoapClient.EndpointConfiguration.DailyInfoSoap12);
    var now = DateTime.Now;
    var response = client.KeyRateAsync(now.AddDays(-7), now).Result;
    var lastDate = DateTime.MinValue;
    var lastRate = 0.0M;
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
            if (date > lastDate) {
              lastDate = date;
              lastRate = rate;
            }
            // Console.WriteLine($"{date.ToString("s")}: {rate}");
          }
        }
      }
    }
    Console.WriteLine($"Last key rate {lastRate} (effective from {lastDate.ToString("s")})");
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
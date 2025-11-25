# CBR Key Rate Monitor

Monitor the Central Bank of Russia's key interest rate using either .NET or JavaScript implementations. Both versions periodically fetch the Key Rate from the CBR SOAP API and RSS feed, then print to the console.

## Language Options

Choose between two implementations:

### .NET 8.0 Version (C#)

#### Prerequisites
- [Install .NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

#### How To Use
1. Navigate to your project folder in terminal/command prompt
2. Restore packages: `dotnet restore`
3. Run: `dotnet run`

### JavaScript Version (Node.js)

#### Prerequisites
- [Install Node.js 18+](https://nodejs.org/)

#### How To Use
1. Navigate to your project folder in terminal/command prompt
2. Install dependencies: `npm install`
3. Run: `npm start`

## Features

Both implementations provide:
- Fetches key rate from CBR SOAP API with historical data
- Parses CBR RSS feed for rate announcements
- Runs every 2 hours automatically
- Shows effective date vs. publication date
- Graceful shutdown on Ctrl+C

The application will run indefinitely until you stop it manually, fetching the key rate every 2 hours.

### Example output

```
$ npm start  # or dotnet run
CBR Key Rate Monitor (JavaScript Version)
Starting periodic checks every 2 hours...
API: Last key rate 18.00 (effective from 2025-07-28T02:30:00)
RSS: Last key rate 18.00 (published at 2025-07-25T00:00:00)
Monitor running. Press Ctrl+C to stop.
```

## Update schema

```
rm -rf ServiceReference
dotnet-svcutil https://www.cbr.ru/DailyInfoWebServ/DailyInfo.asmx?WSDL
```

## Contact

For any inquiries or suggestions, please open an issue on GitHub.
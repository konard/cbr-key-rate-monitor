# CBR Key Rate Monitor

This project provides both .NET 8.0 and Node.js implementations that periodically fetch the Key Rate from the Central Bank of Russia (CBR) SOAP API and RSS feed, then print to the console.

## .NET 8.0 Version

### Prerequisites

You need to install the following software to run the .NET version:

1. [Install .NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### How To Use (.NET)

- Clone the repository or download the code.

- Navigate to your project folder in your terminal/command prompt.

- Restore the necessary packages by running this command:

  ```sh
  dotnet restore
  ```

- Execute the following command to run your console application:

  ```sh
  dotnet run
  ```

## Node.js Version

### Prerequisites

You need to install the following software to run the Node.js version:

1. [Install Node.js](https://nodejs.org/) (version 16 or higher)

### How To Use (Node.js)

- Clone the repository or download the code.

- Navigate to your project folder in your terminal/command prompt.

- Install the necessary packages by running this command:

  ```sh
  npm install
  ```

- Execute the following command to run the application:

  ```sh
  npm start
  ```

  Or directly with Node.js:

  ```sh
  node index.js
  ```

## Functionality

Both versions will fetch the key rate from both the CBR API and RSS feed every 2 hours and output it to the console. The API shows when the current rate became effective, while the RSS shows the publication date of rate announcements.

Please note both applications will run indefinitely until you stop them manually. This is done by design to ensure that the application fetches the key rate every 2 hours.

### Example output

```
$ npm start
CBR Key Rate Monitor started...
API: Last key rate 18.00 (effective from 2025-07-28T02:30:00)
RSS: Last key rate 18.00 (published at 2025-07-25T00:00:00)
Monitor running... Press Ctrl+C to stop.
```

## Update schema

```
rm -rf ServiceReference
dotnet-svcutil https://www.cbr.ru/DailyInfoWebServ/DailyInfo.asmx?WSDL
```

## Contact

For any inquiries or suggestions, please open an issue on GitHub.
# CBR Key Rate Monitor

This is a .NET 6.0 console application that periodically fetches the Key Rate from the Central Bank of Russia (CBR) SOAP API and prints to the console.

## Prerequisites

You need to install the following software to run this program:

1. [Install .NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
   
## How To Use

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

The application will now fetch the key rate every 2 hours and output it to the console.

Please note this application will run indefinitely until you stop it manually. This is done by design to ensure that the application fetches the key rate every 2 hours.

### Example output

```
$ dotnet run
API: Last key rate 15.00 (effective from 2023-12-15T00:00:00)
RSS: Last key rate 16.00 (published at 2023-12-15T00:00:00)
```

## Update schema

```

dotnet-svcutil https://www.cbr.ru/DailyInfoWebServ/DailyInfo.asmx?WSDL
```

## Contact

For any inquiries or suggestions, please open an issue on GitHub.
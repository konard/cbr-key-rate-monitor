import soap from 'soap';
import Parser from 'rss-parser';
import xml2js from 'xml2js';

// Russian culture settings for date parsing
const ruCulture = new Intl.DateTimeFormat('ru-RU');

async function checkKeyRateAPI() {
  try {
    console.log('Fetching key rate from CBR SOAP API...');

    const url = 'https://www.cbr.ru/DailyInfoWebServ/DailyInfo.asmx?WSDL';
    const client = await soap.createClientAsync(url);

    const now = new Date();
    const thirtyDaysAgo = new Date(now.getTime() - (30 * 24 * 60 * 60 * 1000));

    // Format dates as YYYY-MM-DD
    const fromDate = thirtyDaysAgo.toISOString().split('T')[0];
    const toDate = now.toISOString().split('T')[0];

    const result = await client.KeyRateAsync({
      fromDate: fromDate,
      ToDate: toDate
    });

    const rates = [];

    // Parse the XML response - the result structure might be different
    if (result && result[0]) {
      let xmlData = result[0];

      // Try different possible response structures
      if (xmlData.KeyRateResult) {
        xmlData = xmlData.KeyRateResult;
      }

      // If xmlData is a string, parse it
      if (typeof xmlData === 'string') {
        const parser = new xml2js.Parser({
          explicitArray: false,
          ignoreAttrs: true,
          trim: true
        });

        try {
          const parsedXml = await parser.parseStringPromise(xmlData);

          // Navigate through possible XML structures
          let keyRateData = parsedXml;
          if (parsedXml.diffgram) {
            keyRateData = parsedXml.diffgram;
          }
          if (keyRateData.NewDataSet) {
            keyRateData = keyRateData.NewDataSet;
          }
          if (keyRateData.KeyRate) {
            const keyRates = Array.isArray(keyRateData.KeyRate) ? keyRateData.KeyRate : [keyRateData.KeyRate];

            for (const kr of keyRates) {
              if (kr.DT && kr.Rate) {
                const date = new Date(kr.DT);
                const rate = parseFloat(kr.Rate);
                if (!isNaN(rate) && date instanceof Date && !isNaN(date)) {
                  rates.push({ date, rate });
                }
              }
            }
          }
        } catch (parseError) {
          console.error('XML parsing error:', parseError.message);
          return;
        }
      } else {
        // If it's already an object, try to extract data directly
        console.log('API response structure:', JSON.stringify(xmlData, null, 2).substring(0, 500) + '...');
      }
    }

    // Sort by date
    rates.sort((a, b) => a.date - b.date);

    if (rates.length > 0) {
      const lastRate = rates[rates.length - 1].rate;
      const effectiveFromDate = rates.find(r => r.rate === lastRate)?.date;

      if (effectiveFromDate) {
        console.log(`API: Last key rate ${lastRate} (effective from ${effectiveFromDate.toISOString()})`);
      }
    } else {
      console.log('API: No key rate data found in response');
    }
  } catch (error) {
    console.error('Error fetching from API:', error.message);
  }
}

async function checkKeyRateRssFeed() {
  try {
    console.log('Fetching key rate from CBR RSS feed...');

    const parser = new Parser();
    const feed = await parser.parseURL('https://www.cbr.ru/rss/RssPress');

    let lastDate = new Date(0); // Minimum date
    let lastRate = 0.0;

    for (const item of feed.items) {
      if (item.title && item.title.includes('ключевую ставку')) {
        // Use regex to get the key rate value and date from the title
        const regex = /до\s+(?<rate>\d+(,\d+)?)%\s+годовых\s+\((?<date>\d{2}\.\d{2}\.\d{4})\)/;
        const match = item.title.match(regex);

        if (match && match.groups) {
          const rate = parseFloat(match.groups.rate.replace(',', '.'));
          const dateParts = match.groups.date.split('.');
          const date = new Date(
            parseInt(dateParts[2]), // year
            parseInt(dateParts[1]) - 1, // month (0-indexed)
            parseInt(dateParts[0]) // day
          );

          if (date > lastDate) {
            lastDate = date;
            lastRate = rate;
          }
        }
      }
    }

    console.log(`RSS: Last key rate ${lastRate} (published at ${lastDate.toISOString()})`);
  } catch (error) {
    console.error('Error fetching from RSS:', error.message);
  }
}

async function listAllKeyRates() {
  try {
    console.log('Listing all key rates...');

    const url = 'https://www.cbr.ru/DailyInfoWebServ/DailyInfo.asmx?WSDL';
    const client = await soap.createClientAsync(url);

    const result = await client.KeyRateAsync({
      fromDate: '2023-08-01',
      ToDate: '2023-12-19'
    });

    if (result && result[0] && result[0].KeyRateResult) {
      const xmlString = result[0].KeyRateResult;
      const parser = new xml2js.Parser();
      const parsedXml = await parser.parseStringPromise(xmlString);

      if (parsedXml && parsedXml.diffgram && parsedXml.diffgram.NewDataSet) {
        const keyRates = parsedXml.diffgram.NewDataSet[0].KeyRate || [];

        for (const kr of keyRates) {
          if (kr.DT && kr.Rate) {
            const date = new Date(kr.DT[0]);
            const rate = parseFloat(kr.Rate[0]);
            console.log(`${date.toISOString()}: ${rate}`);
          }
        }
      }
    }
  } catch (error) {
    console.error('Error listing key rates:', error.message);
  }
}

async function main() {
  console.log('CBR Key Rate Monitor (JavaScript Version)');
  console.log('Starting periodic checks every 2 hours...');

  // Run initial check
  await checkKeyRateAPI();
  await checkKeyRateRssFeed();

  // Set up timer to check every 2 hours (120 minutes)
  setInterval(async () => {
    await checkKeyRateAPI();
    await checkKeyRateRssFeed();
  }, 120 * 60 * 1000);

  // Keep the process running
  console.log('Monitor running. Press Ctrl+C to stop.');
}

// Handle graceful shutdown
process.on('SIGINT', () => {
  console.log('\nGracefully shutting down...');
  process.exit(0);
});

process.on('SIGTERM', () => {
  console.log('\nGracefully shutting down...');
  process.exit(0);
});

// Start the application
main().catch((error) => {
  console.error('Application error:', error);
  process.exit(1);
});
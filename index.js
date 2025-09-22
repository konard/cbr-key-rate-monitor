import soap from 'soap';
import fetch from 'node-fetch';
import { parseString } from 'xml2js';

const CBR_SOAP_URL = 'https://www.cbr.ru/DailyInfoWebServ/DailyInfo.asmx?WSDL';
const CBR_RSS_URL = 'https://www.cbr.ru/rss/RssPress';

/**
 * Fetches key rate from CBR SOAP API
 */
async function checkKeyRateAPI() {
  try {
    console.log('Fetching key rate from CBR SOAP API...');

    const client = await soap.createClientAsync(CBR_SOAP_URL);
    const now = new Date();
    const fromDate = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000); // 30 days ago

    const result = await client.KeyRateAsync({
      fromDate: fromDate.toISOString().split('T')[0],
      ToDate: now.toISOString().split('T')[0]
    });

    const rates = [];

    if (result && result[0] && result[0].KeyRateResult && result[0].KeyRateResult.diffgram) {
      const keyRates = result[0].KeyRateResult.diffgram.KeyRate;

      if (keyRates && keyRates.KR) {
        for (const krData of keyRates.KR) {
          const date = new Date(krData.DT);
          const rate = parseFloat(krData.Rate);
          rates.push({ date, rate });
        }
      }
    }

    // Sort by date
    rates.sort((a, b) => a.date - b.date);

    if (rates.length > 0) {
      const lastRate = rates[rates.length - 1].rate;
      const effectiveFromDate = rates.find(r => r.rate === lastRate).date;
      console.log(`API: Last key rate ${lastRate} (effective from ${effectiveFromDate.toISOString()})`);
    } else {
      console.log('API: No key rate data found');
    }
  } catch (error) {
    console.error('Error fetching key rate from API:', error.message);
  }
}

/**
 * Fetches key rate from CBR RSS feed
 */
async function checkKeyRateRssFeed() {
  try {
    console.log('Fetching key rate from CBR RSS feed...');

    const response = await fetch(CBR_RSS_URL);
    const xmlText = await response.text();

    parseString(xmlText, (err, result) => {
      if (err) {
        console.error('Error parsing RSS feed:', err.message);
        return;
      }

      let lastDate = new Date(0);
      let lastRate = 0;

      if (result && result.rss && result.rss.channel && result.rss.channel[0].item) {
        const items = result.rss.channel[0].item;

        for (const item of items) {
          const title = item.title[0];

          if (title.includes('ключевую ставку')) {
            // Use regex to get the key rate value and date from the title
            const regex = /до\s+(?<rate>\d+(,\d+)?)%\s+годовых\s+\((?<date>\d{2}\.\d{2}\.\d{4})\)/;
            const match = title.match(regex);

            if (match) {
              const rate = parseFloat(match.groups.rate.replace(',', '.'));
              const dateParts = match.groups.date.split('.');
              const date = new Date(dateParts[2], dateParts[1] - 1, dateParts[0]); // DD.MM.YYYY to Date

              if (date > lastDate) {
                lastDate = date;
                lastRate = rate;
              }
            }
          }
        }
      }

      console.log(`RSS: Last key rate ${lastRate} (published at ${lastDate.toISOString()})`);
    });
  } catch (error) {
    console.error('Error fetching key rate from RSS feed:', error.message);
  }
}

/**
 * Main function that runs the monitoring
 */
async function main() {
  console.log('CBR Key Rate Monitor started...');

  // Run immediately on start
  await checkKeyRateAPI();
  await checkKeyRateRssFeed();

  // Set up timer to run every 2 hours (7200000 milliseconds)
  setInterval(async () => {
    console.log('\n--- Checking key rates ---');
    await checkKeyRateAPI();
    await checkKeyRateRssFeed();
  }, 2 * 60 * 60 * 1000);

  console.log('Monitor running... Press Ctrl+C to stop.');
}

// Handle graceful shutdown
process.on('SIGINT', () => {
  console.log('\nShutting down CBR Key Rate Monitor...');
  process.exit(0);
});

process.on('SIGTERM', () => {
  console.log('\nShutting down CBR Key Rate Monitor...');
  process.exit(0);
});

// Start the application
main().catch(console.error);
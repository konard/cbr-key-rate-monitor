import soap from 'soap';
import Parser from 'rss-parser';

console.log('Testing JavaScript CBR Key Rate Monitor...');

async function testSoapClient() {
  try {
    console.log('Testing SOAP client connection...');
    const url = 'https://www.cbr.ru/DailyInfoWebServ/DailyInfo.asmx?WSDL';
    const client = await soap.createClientAsync(url);
    console.log('✓ SOAP client created successfully');
    console.log('Available methods:', Object.keys(client).filter(key => typeof client[key] === 'function').slice(0, 5));
    return true;
  } catch (error) {
    console.error('✗ SOAP client error:', error.message);
    return false;
  }
}

async function testRssParser() {
  try {
    console.log('Testing RSS parser...');
    const parser = new Parser();
    const feed = await parser.parseURL('https://www.cbr.ru/rss/RssPress');
    console.log('✓ RSS feed parsed successfully');
    console.log(`Found ${feed.items.length} items in feed`);

    // Look for key rate items
    const keyRateItems = feed.items.filter(item =>
      item.title && item.title.includes('ключевую ставку')
    );
    console.log(`Found ${keyRateItems.length} key rate related items`);

    if (keyRateItems.length > 0) {
      console.log('Latest key rate item:', keyRateItems[0].title.substring(0, 100) + '...');
    }

    return true;
  } catch (error) {
    console.error('✗ RSS parser error:', error.message);
    return false;
  }
}

async function runTests() {
  console.log('Running basic connectivity tests...\n');

  const soapTest = await testSoapClient();
  console.log('');
  const rssTest = await testRssParser();

  console.log('\n--- Test Results ---');
  console.log(`SOAP API: ${soapTest ? 'PASS' : 'FAIL'}`);
  console.log(`RSS Feed: ${rssTest ? 'PASS' : 'FAIL'}`);

  if (soapTest && rssTest) {
    console.log('\n✓ All tests passed! The JavaScript implementation should work correctly.');
  } else {
    console.log('\n✗ Some tests failed. Check network connectivity and API availability.');
  }
}

runTests().catch(console.error);
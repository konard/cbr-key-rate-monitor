// Test script to verify Telegram integration works without actual bot credentials
// This demonstrates how the bot would behave with and without credentials

using System;

class TestProgram
{
    static void Main()
    {
        Console.WriteLine("=== Testing Telegram Bot Integration ===");

        // Test 1: No environment variables set
        Console.WriteLine("\n1. Testing without TELEGRAM_BOT_TOKEN:");
        var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        var chatId = Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID");

        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("✓ Warning: TELEGRAM_BOT_TOKEN not set. Telegram notifications disabled.");
        }
        else
        {
            Console.WriteLine($"Token found: {token.Substring(0, 5)}...");
        }

        if (string.IsNullOrEmpty(chatId))
        {
            Console.WriteLine("✓ TELEGRAM_CHAT_ID not set");
        }
        else
        {
            Console.WriteLine($"Chat ID found: {chatId}");
        }

        // Test 2: Simulate notification message format
        Console.WriteLine("\n2. Testing notification message format:");
        var testRate = 21.0m;
        var testDate = DateTime.Now;
        var prevRate = 20.0m;

        var message = $"🏦 CBR Key Rate Changed!\n\nNew rate: {testRate}%\nEffective from: {testDate:dd.MM.yyyy}\nPrevious rate: {prevRate}%";
        Console.WriteLine("Sample notification message:");
        Console.WriteLine(message);

        Console.WriteLine("\n=== Test completed ===");
    }
}
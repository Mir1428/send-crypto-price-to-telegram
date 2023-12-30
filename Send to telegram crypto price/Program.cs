using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Send_to_telegram_crypto_price
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

            do
            {
                var price = GetPriceAsync();
                Console.WriteLine(SendPriceToTelegram(price.Result).Result);
                Console.WriteLine($"Wait for {configuration.GetSection("PeriodOfTimeMinutes").Value} min");
                Thread.Sleep(TimeSpan.FromMinutes(int.Parse(configuration.GetSection("PeriodOfTimeMinutes").Value)));
            } while (true);
        }

        public static async Task<string> GetPriceAsync()
        {
            try
            {
                var document = new HtmlDocument();
                var client = new HttpClient();
                StringBuilder stringBuilder = new StringBuilder();

                var responseChax = await client.GetStringAsync("https://kifpool.me/markets/newest");
                document.LoadHtml(responseChax);
                var elementChax = document.DocumentNode.SelectSingleNode("//*[@id=\"table-scroll\"]/div/table/tbody/tr[8]/td[2]").SelectSingleNode("//*[@id=\"table-scroll\"]/div/table/tbody/tr[8]/td[2]/div");
                if (elementChax != null)
                {
                    Console.WriteLine("Get Price CHAXUSDT");
                    stringBuilder.AppendLine($"CHAXUSD: {elementChax.InnerHtml}");
                }

                var responseUsdt = await client.GetStringAsync("https://kifpool.me/markets");
                document.LoadHtml(responseUsdt);
                var elementUsdt = document.DocumentNode.SelectSingleNode("//*[@id=\"table-scroll\"]/div/table/tbody/tr[4]/td[4]/text()");
                if (elementUsdt != null)
                {
                    Console.WriteLine("Get Price USDT");
                    stringBuilder.AppendLine($"USDT: {elementUsdt.InnerHtml}");
                }

                return stringBuilder.ToString();

            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Exception(Get price Service): {ex.Message}");
                return null;
            }
        }

        public static async Task<string> SendPriceToTelegram(string message)
        {
            try
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                         .SetBasePath(Directory.GetCurrentDirectory())
                         .AddJsonFile("appsettings.json")
                         .Build();
                var botToken = configuration.GetSection("BotToken").Value;
                var chatId = configuration.GetSection("ChatId").Value;

                if (botToken != null && chatId != null && !string.IsNullOrEmpty(message))
                {
                    var botClient = new TelegramBotClient(botToken);
                    await botClient.SendTextMessageAsync(chatId: chatId, text: message);
                    await Console.Out.WriteLineAsync("");
                    return "Price sent to Telegram";
                }
                return "Price not sent to Telegram";
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Exception(Telegram Service): {ex.Message}");
                return null;
            };
        }
    }
}

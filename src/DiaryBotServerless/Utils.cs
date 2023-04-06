using FireSharp;
using FireSharp.Config;
using FireSharp.Response;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiaryBotServerless;

public class Utils
{
    public static async Task<long> GetChannelId(string channelName)
    {
        var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
        var channelid = -1L;
        using var httpClient = new HttpClient();
        var response =
            await httpClient.GetAsync($"https://api.telegram.org/bot{botToken}/getChat?chat_id={channelName}");
        var responseContent = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(responseContent);
        if (json.ContainsKey("result"))
        {
            channelid = json["result"]["id"].Value<long>();
            return channelid;
        }

        Console.WriteLine("Ошибка при получении ID канала");
        return -1;
    }

    
    

    public static async void SetBotCommands(TelegramBotClient client)
    {
        BotCommand botCommand = new BotCommand{Command = "write_post", Description = "Написать пост в каннал"};
        IEnumerable<BotCommand> s = new[] {botCommand};
        await client.SetMyCommandsAsync(s);
    }

   
    public static string FormatPost(User user, string postText)
    {
        
        const string daysWithBotField = "daysWithBot";
        string date = DateTime.Now.ToShortDateString();
        return $"{date}   ({GetNumberEmoji(user.PostCount)})\n \n" +
               $"{postText}" +
               $"\n \n✅Написано с помощью DiaryBot🤖";
    }
    public static string GetNumberEmoji(int number)
    {
        var numsArray = SplitToDigits(number);
        var outputString = "#️⃣";
        for (int i = 0; i < numsArray.Length; i++)
        {
            switch (numsArray[i])
            {
                case 1 : outputString += "1️⃣"; break;
                case 2 : outputString += "2️⃣"; break;
                case 3 : outputString += "3️⃣"; break;
                case 4 : outputString += "4️⃣"; break;
                case 5 : outputString += "5️⃣"; break;
                case 6 : outputString += "6️⃣"; break;
                case 7 : outputString += "7️⃣"; break; 
                case 8 : outputString += "8️⃣"; break; 
                case 9 : outputString += "9️⃣"; break;
                case 0 : outputString += "0️⃣"; break;
            }
        }

        return outputString;
    }
    static int[] SplitToDigits(int number)
    {
        number = Math.Abs(number);
        //Если число меньше 10
        if (number < 10)
        {
            return new int[] { number };
        }
        //Результирующий массив размером в количество цифр в числе
        var result = new int[(int)Math.Log10(number) + 1];
        for (int i = 0; i < result.Length; i++)
        {
            //Последняя цифра числа как остаток от деления на 10
            result[result.Length - i - 1] = number % 10;
            //уменьшаем исходное число в 10 раз
            number /= 10;
        }
        return result;
    }
}
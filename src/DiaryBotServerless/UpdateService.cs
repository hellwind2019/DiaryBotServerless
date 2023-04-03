using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DiaryBotServerless;

public class UpdateService
{
    private readonly TelegramBotClient _botClient;
    private readonly DynamoDBService _dynamoDbService;
    private BotStateMachine _botStateMachine;
    public string BucketName = "diary-bot-bucket";

    public UpdateService()
    {
        // replace with your bot token
        var token = Environment.GetEnvironmentVariable("BOT_TOKEN")!;
        _botClient = new TelegramBotClient(token);
        _dynamoDbService = new DynamoDBService();
    }
    public async Task EchoAsync(Update update)
    {
        
        var message = update.Message;
        LambdaLogger.Log("Received Message from " + message.Chat.Id);
        var User = await _dynamoDbService.GetUserByIdAsync(update.Message.Chat.Id);
        LambdaLogger.Log("Update : \n\n" +update);
        _botStateMachine = new BotStateMachine(update, User, _botClient, _dynamoDbService);
        
        try{
            switch (message.Text)
            {
                //you
                case "/start":
                    _botStateMachine.Start();
                    LambdaLogger.Log("Received Message /start");
                    break;
                case { } a when a.Contains('@'):
                    _botStateMachine.ChannelNameReceived();
                    break;
                case "Готово✅":
                    _botStateMachine.BotAdded();
                    break;
                case "Вижу😀":
                    _botStateMachine.TestMessageSeen();
                    break;
                case "Не вижу ☹":
                    _botStateMachine.TestMessageError();
                    break;

            }
        }
        
        /*try
        {
            var currentUser = await _dynamoDbService.GetUserByIdAsync(update.Message.Chat.Id);
            if (!currentUser.IsRegistered)
            {
                if (message.Text == "/start")
                {

                }
                else if (message.Text.Contains("@"))
                {
                    
                }
                else if (message.Text.Contains("Готово✅"))
                {
                    
                }
                else if (message.Text.Contains("Вижу😀"))
                {
                    
                }
                else if (message.Text.Contains("Не вижу ☹"))
                {
                    
                    
                }
            }
            else if (currentUser.IsRegistered)
            {
                if (message.Text == "Запостить✅")
                {
                    await _botClient.SendTextMessageAsync(currentUser.ChannelId, currentUser.CurrentPostText);
                    await _botClient.SendTextMessageAsync(message.Chat.Id, "Пост уже на канале 😉",
                        replyMarkup: new ReplyKeyboardRemove());
                    currentUser.IsPostingNow = false;
                    currentUser.CurrentPostText = "";
                    await _dynamoDbService.AddUserAsync(currentUser);
                }
                else if (message.Text == "Редактировать✏️️")
                {
                    await _botClient.SendTextMessageAsync(message.Chat.Id, "Напишите исправленый текст: ");
                }
                else if (currentUser.IsPostingNow)
                {
                    await _botClient.SendTextMessageAsync(message.Chat.Id, "Так будет выглядеть пост :");
                    currentUser.CurrentPostText = Utils.FormatPost(currentUser, message.Text);
                    await _dynamoDbService.AddUserAsync(currentUser);
                   
                    await _botClient.SendTextMessageAsync(message.Chat.Id, currentUser.CurrentPostText,
                        replyMarkup: replyKeyboardMarkup);
                }

                if (message.Text.Contains("/write_post"))
                {
                    await _botClient.SendTextMessageAsync(message.Chat.Id, "Как прошел день?");
                    currentUser.IsPostingNow = true;
                    await _dynamoDbService.AddUserAsync(currentUser);
                }
                
            }
            else
            {
                var answer = "Что делать то?";
                await _botClient.SendTextMessageAsync(message.Chat.Id, answer);
            }
        }*/
        catch (Exception e)
        {
            await _botClient.SendTextMessageAsync(message.Chat.Id, e.ToString());
            throw;
        }
    }
    
}
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
        if (update is null) return;
        if (!(update.Message is { } message)) return;
        LambdaLogger.Log("Received Message from " + message.Chat.Id);
        /*var firebaseClient = Utils.GetFirebaseClient();
        const string isStartedField = "isStarted";
        const string channelIdField = "channelID";
        const string isRegisteredField = "isRegistered";
        const string isPostedTodayField = "isPostedToday";
        const string isPostingNowField = "isPostingNow";
        const string currentPostTextField = "currentPostText";
        const string daysWithBotField = "daysWithBot";
        if (message?.Text != null)
        {
    
        var isRegistered = await Utils.GetRegisterStatus(firebaseClient, message);
        var isPostingNow = await Utils.GetUserField(message.Chat.Id,isPostingNowField);
        var isPostedToday = await Utils.GetUserField(message.Chat.Id, isPostedTodayField);
        if (isRegistered != "true")
        {
            var isStarted = await Utils.GetStartStatus(firebaseClient, message);
            if (message.Text == "/start")
            {
                if (isStarted == "true")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Welcome back, {message.Chat.FirstName}");
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Bot started...");
                    await firebaseClient.SetAsync($"Users/{message.Chat.Id}/{isStartedField}", true);
                    await botClient.SendTextMessageAsync(message.Chat.Id,
                        "Send your channel name like\"@channel_name\"");
                }
            }

            if (message.Text.Contains("@") && isStarted == "true")
            {
                var channelId = Utils.GetChannelId(Utils.GetBotToken(), message.Text).Result;
                Console.WriteLine(channelId);
                await firebaseClient.SetAsync($"Users/{message.Chat.Id}/{channelIdField}", channelId);
                await botClient.SendTextMessageAsync(message.Chat.Id, "Channel registered");
                await botClient.SendTextMessageAsync(message.Chat.Id, "Now add bot to this channel as admin");

                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                {
                    new KeyboardButton[] { "Done" }
                })
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Click this button, when you added bot to channel",
                    replyMarkup: replyKeyboardMarkup
                );
            }

            if (message.Text == "Done")
            {
                var firebaseResponse = await firebaseClient.GetAsync($"Users/{message.Chat.Id}/{channelIdField}");
                var responseId = firebaseResponse.ResultAs<long>();

                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                {
                    new KeyboardButton[] { "Yes, i see", "No, i can't see the message" }
                })
                {
                    ResizeKeyboard = true
                };

                await botClient.SendTextMessageAsync(message.Chat.Id, "Now bot will send a message to you channel",
                    replyMarkup: new ReplyKeyboardRemove());
                await botClient.SendTextMessageAsync(responseId, "Test message in channel");
                await botClient.SendTextMessageAsync(message.Chat.Id, "See the message from the bot in the channel?",
                    replyMarkup: replyKeyboardMarkup);
            }

            if (message.Text == "Yes, i see")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Congratulations! Now your channel is registered",
                    replyMarkup: new ReplyKeyboardRemove());
                await firebaseClient.SetAsync($"Users/{message.Chat.Id}/{isRegisteredField}", true);
                await firebaseClient.SetAsync($"Users/{message.Chat.Id}/{isPostedTodayField}", false);
                await firebaseClient.SetAsync($"Users/{message.Chat.Id}/{daysWithBotField}", 1); 
            }
        }

        if (message.Text == "/write_post")
        {
            
            await botClient.SendTextMessageAsync(message.Chat.Id, "Как прошел день?");
            await firebaseClient.SetAsync($"Users/{message.Chat.Id}/{isPostingNowField}", true);
        }
        if (isPostingNow == "true")
        {
            
            await Utils.SetUserField(message.Chat.Id, currentPostTextField, message.Text);
            await botClient.SendTextMessageAsync(message.Chat.Id, "Так будет выглядеть твой пост : ");
            var postText = Utils.FormatPost(message.Chat.Id, Utils.GetUserField(message.Chat.Id, currentPostTextField).Result);
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                new KeyboardButton[] { "Запостить ✅", "Отмена ❌" }
            })
            {
                ResizeKeyboard = true
            };
            await Utils.SetUserField(message.Chat.Id, isPostingNowField, false);
            await botClient.SendTextMessageAsync(message.Chat.Id, postText,replyMarkup: replyKeyboardMarkup );
        }
        if (message.Text == "Запостить ✅")
        {
            var channelId = long.Parse(Utils.GetUserField( message.Chat.Id, channelIdField).Result);
            var postText = Utils.FormatPost(message.Chat.Id, Utils.GetUserField(message.Chat.Id, currentPostTextField).Result);
            await botClient.SendTextMessageAsync(channelId, postText);
            await botClient.SendTextMessageAsync(message.Chat.Id, "Твой пост уже на канале ✅",
                replyMarkup: new ReplyKeyboardRemove());
            await Utils.SetUserField(message.Chat.Id, currentPostTextField, "");
            if (isPostedToday == "false")
            {
                await Utils.SetUserField(message.Chat.Id, isPostedTodayField, true);
                Utils.IncreaseDayWithBot(message.Chat.Id);
            }
           
        }
        if (message.Text =="Отмена ❌")
        {
            await Utils.SetUserField(message.Chat.Id, currentPostTextField, "");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Окей, нет, так нет", replyMarkup: new ReplyKeyboardRemove());
        }
    }*/
        var awsAccessKeyId = Environment.GetEnvironmentVariable("ACCESS_KEY");
        var awsSecretAccessKey = Environment.GetEnvironmentVariable("SECRET_KEY");
        
        
        try
        {
            var currentUser = await _dynamoDbService.GetUserByIdAsync(update.Message.Chat.Id);
            if (!currentUser.IsRegistered)
            {
                if (message.Text == "/start")
                {
                    await _dynamoDbService.AddUserAsync(new User(message.Chat.Id));
                    var answer = "Отправь ссылку на свой дневник в формате @my_diary";
                    await _botClient.SendTextMessageAsync(message.Chat.Id, answer);
                }
                else if (message.Text.Contains("@"))
                {
                    var channelName = message.Text;
                    var channelId = await Utils.GetChannelId(channelName);
                    var user = await _dynamoDbService.GetUserByIdAsync(message.Chat.Id);
                    user.ChannelId = channelId;
                    await _dynamoDbService.AddUserAsync(user);
                    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                    {
                        new KeyboardButton[] { "Готово✅" }
                    })
                    {
                        ResizeKeyboard = true
                    };
                    var answer = "Отлично! Теперь добавь бота в канал, и дай ему роль администратора";
                    await _botClient.SendTextMessageAsync(message.Chat.Id, answer, replyMarkup: replyKeyboardMarkup);
                }
                else if (message.Text.Contains("Готово✅"))
                {
                    var user = await _dynamoDbService.GetUserByIdAsync(message.Chat.Id);
                    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                    {
                        new KeyboardButton[] { "Вижу😀", "Не вижу ☹" }
                    })
                    {
                        ResizeKeyboard = true
                    };
                    var answer = $"Теперь бот отправит тестовое смс в канал";
                    await _botClient.SendTextMessageAsync(message.Chat.Id, answer, replyMarkup: replyKeyboardMarkup);
                    var channelAnswer = "Тестовое сообщение от DiaryBot";
                    await _botClient.SendTextMessageAsync(user.ChannelId, channelAnswer);
                }
                else if (message.Text.Contains("Вижу😀"))
                {
                    var answer = "Красавчик, ты смог!";
                    var user = await _dynamoDbService.GetUserByIdAsync(message.Chat.Id);
                    user.IsRegistered = true;
                    await _dynamoDbService.AddUserAsync(user);
                    await _botClient.SendTextMessageAsync(message.Chat.Id, answer,
                        replyMarkup: new ReplyKeyboardRemove());
                }
                else if (message.Text.Contains("Не вижу ☹"))
                {
                    var answer = "Не для лохов делалось";
                    await _botClient.SendTextMessageAsync(message.Chat.Id, answer,
                        replyMarkup: new ReplyKeyboardRemove());
                    
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
                    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                    {
                        new KeyboardButton[] { "Запостить✅", "Редактировать✏️️" }
                    })
                    {
                        ResizeKeyboard = true
                    };
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
        }
        catch (Exception e)
        {
            await _botClient.SendTextMessageAsync(message.Chat.Id, e.ToString());
            throw;
        }
    }
    
}
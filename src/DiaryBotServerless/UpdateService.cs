using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using DiaryBotServerless;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = Telegram.Bot.Types.File;

namespace SimpleApi;

public class UpdateService
{
    private readonly TelegramBotClient botClient;
    public string BucketName = "diary-bot-bucket";
    public UpdateService()
    {
        // replace with your bot token
        string token = Environment.GetEnvironmentVariable("BOT_TOKEN")!;
        botClient = new TelegramBotClient(token);
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
        var awsAccessKeyId = "AKIATHMI7NGKATDASD76";
        var awsSecretAccessKey = "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";
        
        var s3client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey);
        
        
        switch (message.Type)
        {
            case MessageType.Text:
                switch (message.Text.ToLower())
                {
                    case "привет кабан":
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id,
                            "Здарова кабан братан  " + message.Chat.FirstName);
                        break;
                    }
                    case "bucket test":
                    {
                        var response = await s3client.GetObjectAsync(BucketName, "test_json.json");
                        StreamReader stream;
                        using (stream = new StreamReader(response.ResponseStream))
                        {
                            var text = stream.ReadToEndAsync().Result;
                            await botClient.SendTextMessageAsync(message.Chat.Id, text);
                        }
                        await botClient.SendTextMessageAsync(message.Chat.Id, response.LastModified.ToString());
                        break;
                    }
                    default:
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, message.Text!);
                        break;
                    }
                }

                break;
        }
    }
}
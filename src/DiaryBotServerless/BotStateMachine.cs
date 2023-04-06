using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Machine;
using Appccelerate.StateMachine.AsyncMachine;

using Appccelerate.StateMachine.Machine.Reports;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiaryBotServerless;

public class BotStateMachine
{
    private Update _update = new Update();
    private User? _user;
    private TelegramBotClient _botClient;
    private DynamoDBService _dynamoDbService;
    private Message message;
    private readonly AsyncPassiveStateMachine<States, Events> _stateMachine;

    public BotStateMachine(Update update, User? user, TelegramBotClient botClient, DynamoDBService dynamoDbService, States initialState)
    {
        _update = update;
        _user = user;
        _botClient = botClient;
        message = update.Message;
        _dynamoDbService = dynamoDbService;
        var builder = new Appccelerate.StateMachine.AsyncMachine.StateMachineDefinitionBuilder<States, Events>();
        builder
            .In(States.Idle)
            .On(Events.Start).Goto(States.WaitingForChannelName);

        builder
            .In(States.WaitingForChannelName).ExecuteOnEntry(WaitForChannelName)
            .On(Events.ChannelNameReceived).Goto(States.WaitingForAddingBot);
        
        builder.In(States.WaitingForAddingBot).ExecuteOnEntry(WaitForAddingBot)
            .On(Events.BotAdded).Goto(States.SendingTestMessage);


        builder.WithInitialState(initialState);
        _stateMachine = builder.Build().CreatePassiveStateMachine();
        _stateMachine.Start();
        /*builder
            .In(States.Idle)
            .On(Events.Start)
            .If<bool>(arg => true).Goto(States.MainMenu).Execute(WaitForChannelName)
            .Otherwise().Goto(States.WaitingForChannelName);

        builder
            .In(States.WaitingForChannelName)
            .On(Events.ChannelNameReceived)
            .Goto(States.WaitingForAddingBot).Execute(WaitForAddingBot);
        
        builder
            .In(States.WaitingForAddingBot)
            .On(Events.BotAdded)
            .Goto(States.SendingTestMessage).Execute(SendTestMessage);

        builder
            .In(States.SendingTestMessage)
            .On(Events.TestMessageSeen).Goto(States.MainMenu).Execute(EndOfRegistration)
            .On(Events.TestMessageError).Goto(States.WaitingForChannelName).Execute(SendTestMessageError);

        builder
            .In(States.MainMenu).ExecuteOnEntry(ShowMainMenu);*/

    }

   

    public async Task StartRegister()
    {
        await _stateMachine.Fire(Events.Start);
        await _botClient.SendTextMessageAsync(message.Chat.Id, "StartRegister");
    }
    public async Task ChannelNameReceived()
    {
        await _botClient.SendTextMessageAsync(message.Chat.Id, "ChannelNameReceived");
        await _stateMachine.Fire(Events.ChannelNameReceived);
    }
    public async Task BotAdded()
    { 
       await _stateMachine.Fire(Events.BotAdded);
    }
    
    public async Task TestMessageSeen()
    {
       await _stateMachine.Fire(Events.TestMessageSeen);
    }
  
    public async Task TestMessageError()
    {
       await _stateMachine.Fire(Events.TestMessageError);
    }
   
    
    
    private void ShowMainMenu()
    {
        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
        {
            new KeyboardButton[] { "Новый пост✏", "Статистика📃" }
        })
        {
            ResizeKeyboard = true
        };
        _botClient.SendTextMessageAsync(message.Chat.Id, "Главное меню", replyMarkup:replyKeyboardMarkup);
    }
    private async void WaitForChannelName()
    {
        await _dynamoDbService.AddUserAsync(new User(message.Chat.Id));
        var answer = "Отправь ссылку на свой дневник в формате @my_diary";
        await _botClient.SendTextMessageAsync(message.Chat.Id, answer);
    }

    private async void WaitForAddingBot()
    {
        var channelName = message.Text!;
        var channelId = await Utils.GetChannelId(channelName);
        var user = await _dynamoDbService.GetUserByIdAsync(message.Chat.Id);
        user.ChannelId = channelId;
        user.State = States.WaitingForAddingBot;
       
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

    private async void SendTestMessage()
    {
        var user = await _dynamoDbService.GetUserByIdAsync(message.Chat.Id);
        user.State = States.SendingTestMessage;
        await _dynamoDbService.AddUserAsync(user);
        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
        {
            new KeyboardButton[] { "Вижу😀", "Не вижу ☹" }
        })
        {
            ResizeKeyboard = true
        };
        var answer = "Теперь бот отправит тестовое смс в канал";
        await _botClient.SendTextMessageAsync(message.Chat.Id, answer, replyMarkup: replyKeyboardMarkup);
        var channelAnswer = "Тестовое сообщение от DiaryBot";
        await _botClient.SendTextMessageAsync(user.ChannelId, channelAnswer);
    }
    private async void SendTestMessageError()
    {
        var answer = "Не для лохов делалось";
        await _botClient.SendTextMessageAsync(message.Chat.Id, answer,
            replyMarkup: new ReplyKeyboardRemove());
        var user = await _dynamoDbService.GetUserByIdAsync(message.Chat.Id);
        user.State = States.WaitingForChannelName;
        await _dynamoDbService.AddUserAsync(user);
    }
    private async void EndOfRegistration()
    {
        var answer = "Красавчик, ты смог!";
        var user = await _dynamoDbService.GetUserByIdAsync(message.Chat.Id);
        user.State = States.MainMenu;
        await _dynamoDbService.AddUserAsync(user);
        await _botClient.SendTextMessageAsync(message.Chat.Id, answer,
            replyMarkup: new ReplyKeyboardRemove());
    }


}
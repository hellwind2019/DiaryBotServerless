using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Machine;
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
    private readonly ActiveStateMachine<States, Events> _stateMachine;

    public BotStateMachine(Update update, User? user, TelegramBotClient botClient, DynamoDBService dynamoDbService)
    {
        _update = update;
        _user = user;
        _botClient = botClient;
        message = update.Message;
        _dynamoDbService = dynamoDbService;
        var builder = new StateMachineDefinitionBuilder<States, Events>();
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
        builder
            .In(States.Idle)
            .On(Events.Start).Goto(States.WaitingForChannelName).Execute(WaitForChannelName);

        builder.WithInitialState(States.Idle);
        _stateMachine = builder.Build().CreateActiveStateMachine();
        _stateMachine.Start();
    }

   

    public async void StartRegister()
    {
        bool isRegistered = _user != null;
        _stateMachine.Fire(Events.Start);
    }
    public void ChannelNameReceived()
    {
        _stateMachine.Fire(Events.ChannelNameReceived);
    }
    public void BotAdded()
    { 
        _stateMachine.Fire(Events.BotAdded);
    }
    
    public void TestMessageSeen()
    {
        _stateMachine.Fire(Events.TestMessageSeen);
    }
  
    public void TestMessageError()
    {
        _stateMachine.Fire(Events.TestMessageError);
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
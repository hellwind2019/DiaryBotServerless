namespace DiaryBotServerless;

public enum States
{
    Idle,
    WaitingForChannelName,
    WaitingForAddingBot,
    SendingTestMessage,
    MainMenu
}
public enum Events
{
    Start,
    ChannelNameReceived,
    BotAdded,
    TestMessageSeen,
    TestMessageError
}
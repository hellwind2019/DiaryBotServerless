using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Machine;
using Telegram.Bot.Types;

namespace DiaryBotServerless;

public class BotStateMachine
{
    private Update _update = new Update();
    private enum States
    {
        Idle,
        WaitingForChannelName,
        WaitingForAddingBot,
        SendingTestMessage,
        MainMenu
    }
    private enum Events
    {
        Start,
        CheckRegistration,
        ChannelNameReceived,
        BotAdded,
        TestMessageSeen,
        TestMessageError
    }

    private readonly AsyncPassiveStateMachine<States, Events> _stateMachine;

    public BotStateMachine(Update update)
    {
        _update = update;
        var builder = new StateMachineDefinitionBuilder<States, Events>();
        builder
            .In(States.Idle)
            .On(Events.Start)
            .If(IsRegistered).Goto(States.MainMenu).Execute(ShowMainMenu)
            .Otherwise().Goto(States.WaitingForChannelName).Execute(WaitForChannelName);

        builder
            .In(States.WaitingForChannelName)
            .On(Events.ChannelNameReceived)
            .Goto(States.WaitingForAddingBot);
        
        builder
            .In(States.WaitingForAddingBot)
            .On(Events.BotAdded)
            .Goto(States.SendingTestMessage);

        builder
            .In(States.SendingTestMessage)
            .On(Events.TestMessageSeen).Goto(States.MainMenu)
            .On(Events.TestMessageError).Goto(States.WaitingForChannelName);

    }
    private bool IsRegistered()
    {
        //TODO: check if user is registered
        return false;
    }

    private void ShowMainMenu()
    {
        //TODO: show main menu
    }
    private void WaitForChannelName()
    {
        //TODO: send message about waiting for channel name
    }
}
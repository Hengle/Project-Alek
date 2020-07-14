using JetBrains.Annotations;

public enum BattleEventType { NewRound, WonBattle, LostBattle }
public struct BattleEvents
{
    public BattleEventType _battleEventType;

    [UsedImplicitly] public BattleEvents(BattleEventType eventType)
    {
        _battleEventType = eventType;
    }

    private static BattleEvents @event;

    public static void Trigger(BattleEventType eventType)
    {
        @event._battleEventType = eventType;
        GameEventsManager.TriggerEvent(@event);
    }
}
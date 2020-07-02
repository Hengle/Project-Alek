public enum BattleEventType { NewRound, WonBattle, LostBattle }
public struct BattleEvent
{
    public BattleEventType _battleEventType;

    public BattleEvent(BattleEventType eventType)
    {
        _battleEventType = eventType;
    }

    private static BattleEvent @event;

    public static void Trigger(BattleEventType eventType)
    {
        @event._battleEventType = eventType;
        GameEventsManager.TriggerEvent(@event);
    }
}
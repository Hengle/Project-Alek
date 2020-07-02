using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GameEventsManager
{
    private static Dictionary<Type, List<IGameEventListenerBase>> subscribersList;

    static GameEventsManager()
    {
        subscribersList = new Dictionary<Type, List<IGameEventListenerBase>>();
    }

    public static void AddListener<GEvent>(IGameEventListener<GEvent> listener) where GEvent : struct
    {
        var eventType = typeof(GEvent);

        if (!subscribersList.ContainsKey(eventType))
            subscribersList[eventType] = new List<IGameEventListenerBase>();
        
        if(!SubscriptionExists(eventType, listener))
            subscribersList[eventType].Add(listener);
    }

    public static void RemoveListener<GEvent>(IGameEventListener<GEvent> listener) where GEvent : struct
    {
        var eventType = typeof(GEvent);

        if (!subscribersList.ContainsKey(eventType)) return;

        List<IGameEventListenerBase> subscribers = subscribersList[eventType];
        
        
        for (var i = 0; i<subscribers.Count; i++)
        {
            if (subscribers[i] != listener) continue;
            
            subscribers.Remove( subscribers[i] );

            if( subscribers.Count == 0 )
                subscribersList.Remove( eventType );
            
            return;
        }
    }
    
    public static void TriggerEvent<GEvent>(GEvent @event) where GEvent : struct
    {
        List<IGameEventListenerBase> list;
        if (!subscribersList.TryGetValue(typeof(GEvent), out list)) {
            Logger.Log("There is no listener for this type");
            return;
        }

        foreach (var t in list)
        {
            ( t as IGameEventListener<GEvent> )?.OnGameEvent( @event );
        }
    }
    
    private static bool SubscriptionExists( Type type, IGameEventListenerBase receiver )
    {
        List<IGameEventListenerBase> receivers;

        return subscribersList.TryGetValue( type, out receivers ) && receivers.Any(t => t == receiver);
    }
}

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

public interface IGameEventListenerBase { }

public interface IGameEventListener<in T> : IGameEventListenerBase
{
    void OnGameEvent( T eventType );
}

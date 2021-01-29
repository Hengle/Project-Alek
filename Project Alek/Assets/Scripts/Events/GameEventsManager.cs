using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways] public static class GameEventsManager
{
    private static readonly Dictionary<Type, List<IGameEventListenerBase>> SubscribersList;

    static GameEventsManager() => SubscribersList = new Dictionary<Type, List<IGameEventListenerBase>>();
    
    public static void AddListener<TGEvent>(IGEventListener<TGEvent> listener) where TGEvent : struct
    {
        var eventType = typeof(TGEvent);

        if (!SubscribersList.ContainsKey(eventType)) SubscribersList[eventType] = new List<IGameEventListenerBase>();
        
        if(!SubscriptionExists(eventType, listener)) SubscribersList[eventType].Add(listener);
    }

    public static void RemoveListener<TGEvent>(IGEventListener<TGEvent> listener) where TGEvent : struct
    {
        var eventType = typeof(TGEvent);

        if (!SubscribersList.ContainsKey(eventType)) return;
        
        var subscribers = SubscribersList[eventType];

        for (var i = 0; i<subscribers.Count; i++)
        {
            if (subscribers[i] != listener) continue;
            
            subscribers.Remove(subscribers[i]);

            if( subscribers.Count == 0 ) SubscribersList.Remove(eventType);
            
            return;
        }
    }
    
    public static void TriggerEvent<TGEvent>(TGEvent @event) where TGEvent : struct
    {
        if (!SubscribersList.TryGetValue(typeof(TGEvent), out var list)) return;
        
        for (var i= list.Count - 1; i >= 0; i--) (list[i] as IGEventListener<TGEvent>)?.OnGameEvent(@event);
    }
    
    private static bool SubscriptionExists(Type type, IGameEventListenerBase receiver)
    {
        return SubscribersList.TryGetValue(type, out var receivers) 
               && receivers.Any(t => t == receiver);
    }
}

public interface IGameEventListenerBase { }

public interface IGEventListener<in T> : IGameEventListenerBase
{
    void OnGameEvent( T eventType );
}

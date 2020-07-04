#if EVENTROUTER_THROWEXCEPTIONS
//#define EVENTROUTER_REQUIRELISTENER // Uncomment this if you want listeners to be required for sending events.
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways] public static class GameEventsManager
{
    private static readonly Dictionary<Type, List<IGameEventListenerBase>> SubscribersList;

    static GameEventsManager() => SubscribersList = new Dictionary<Type, List<IGameEventListenerBase>>();
    
    public static void AddListener<TGEvent>(IGameEventListener<TGEvent> listener) where TGEvent : struct
    {
        var eventType = typeof(TGEvent);

        if (!SubscribersList.ContainsKey(eventType))
            SubscribersList[eventType] = new List<IGameEventListenerBase>();
        
        if(!SubscriptionExists(eventType, listener))
            SubscribersList[eventType].Add(listener);
    }

    public static void RemoveListener<TGEvent>(IGameEventListener<TGEvent> listener) where TGEvent : struct
    {
        var eventType = typeof(TGEvent);

        if (!SubscribersList.ContainsKey(eventType))
#if EVENTROUTER_THROWEXCEPTIONS
throw new ArgumentException( string.Format
( "Removing listener \"{0}\", but the event type \"{1}\" isn't registered.", listener, eventType.ToString() ) );
#else
            return;
#endif

        var subscribers = SubscribersList[eventType];
        
        bool listenerFound;
        listenerFound = false;

        for (var i = 0; i<subscribers.Count; i++)
        {
            if (subscribers[i] != listener) continue;
            
            subscribers.Remove( subscribers[i] );
            listenerFound = true;

            if( subscribers.Count == 0 )
                SubscribersList.Remove( eventType );
            
            return;
        }
        
#if EVENTROUTER_THROWEXCEPTIONS
if( !listenerFound )
throw new ArgumentException( string.Format
( "Removing listener, but the supplied receiver isn't subscribed to event type \"{0}\".", eventType.ToString() ) );
#endif
    }
    
    public static void TriggerEvent<TGEvent>(TGEvent @event) where TGEvent : struct
    {
        if (!SubscribersList.TryGetValue(typeof(TGEvent), out var list)) 
        {
#if EVENTROUTER_REQUIRELISTENER
throw new ArgumentException( string.Format( "Attempting to send event of type \"{0}\", but no listener for this type has been found. Make sure this.Subscribe<{0}>(EventRouter) has been called, or that all listeners to this event haven't been unsubscribed.", typeof( MMEvent ).ToString() ) );
#else
            return;
#endif
        }
        
        for (var i= list.Count - 1; i >= 0; i--) (list[i] as IGameEventListener<TGEvent>)?.OnGameEvent(@event);
    }
    
    private static bool SubscriptionExists( Type type, IGameEventListenerBase receiver )
    {
        return SubscribersList.TryGetValue( type, out var receivers ) 
               && receivers.Any(t => t == receiver);
    }
}

public interface IGameEventListenerBase { }

public interface IGameEventListener<in T> : IGameEventListenerBase
{
    void OnGameEvent( T eventType );
}

#if EVENTROUTER_THROWEXCEPTIONS
//#define EVENTROUTER_REQUIRELISTENER // Uncomment this if you want listeners to be required for sending events.
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
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

        if (!subscribersList.ContainsKey(eventType))
            #if EVENTROUTER_THROWEXCEPTIONS
					throw new ArgumentException( string.Format( "Removing listener \"{0}\", but the event type \"{1}\" isn't registered.", listener, eventType.ToString() ) );
            #else
            return;
            #endif

        List<IGameEventListenerBase> subscribers = subscribersList[eventType];
        
        bool listenerFound;
        listenerFound = false;

        if (listenerFound)
        {
				
        }
        
        for (var i = 0; i<subscribers.Count; i++)
        {
            if (subscribers[i] != listener) continue;
            
            subscribers.Remove( subscribers[i] );
            listenerFound = true;

            if( subscribers.Count == 0 )
                subscribersList.Remove( eventType );
            
            return;
        }
        
                #if EVENTROUTER_THROWEXCEPTIONS
		        if( !listenerFound )
		        {
					throw new ArgumentException( string.Format( "Removing listener, but the supplied receiver isn't subscribed to event type \"{0}\".", eventType.ToString() ) );
		        }
                #endif
    }
    
    public static void TriggerEvent<GEvent>(GEvent @event) where GEvent : struct
    {
        List<IGameEventListenerBase> list;
        if (!subscribersList.TryGetValue(typeof(GEvent), out list)) {
            #if EVENTROUTER_REQUIRELISTENER
			            throw new ArgumentException( string.Format( "Attempting to send event of type \"{0}\", but no listener for this type has been found. Make sure this.Subscribe<{0}>(EventRouter) has been called, or that all listeners to this event haven't been unsubscribed.", typeof( MMEvent ).ToString() ) );
            #else
                return;
            #endif
        }

        
        // foreach (var t in list)
        // {
        //     ( t as IGameEventListener<GEvent> )?.OnGameEvent( @event );
        // }
        for (var i= list.Count - 1; i >= 0; i--)
        {
            ( list[i] as IGameEventListener<GEvent> )?.OnGameEvent( @event );
        }
    }
    
    private static bool SubscriptionExists( Type type, IGameEventListenerBase receiver )
    {
        List<IGameEventListenerBase> receivers;

        return subscribersList.TryGetValue( type, out receivers ) && receivers.Any(t => t == receiver);
    }
}



public interface IGameEventListenerBase { }

public interface IGameEventListener<in T> : IGameEventListenerBase
{
    void OnGameEvent( T eventType );
}

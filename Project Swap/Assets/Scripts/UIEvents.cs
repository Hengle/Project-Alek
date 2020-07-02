using UnityEngine;

public enum UIEventType { UpdateInventoryDisplay }
public struct UIEvents
{
        public UIEventType _eventType;
        public ScriptableObject _character;

        public UIEvents(UIEventType eventType, ScriptableObject character)
        {
                _eventType = eventType;
                _character = character;
        }

        private static UIEvents @event;

        public static void Trigger(UIEventType eventType, ScriptableObject character)
        {
                @event._eventType = eventType;
                @event._character = character;
                GameEventsManager.TriggerEvent(@event);
        }
}
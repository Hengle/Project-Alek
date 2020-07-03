using UnityEngine;

public enum UIEventType { ToggleProfileBox }
public struct UIEvents
{
        public UIEventType _eventType;
        public ScriptableObject _character;
        public GameObject _gameObject;

        public UIEvents(UIEventType eventType, ScriptableObject character, GameObject gameObject)
        {
                _eventType = eventType;
                _character = character;
                _gameObject = gameObject;
        }

        private static UIEvents @event;

        public static void Trigger(UIEventType eventType, ScriptableObject character)
        {
                @event._eventType = eventType;
                @event._character = character;
                GameEventsManager.TriggerEvent(@event);
        }

        public static void Trigger(UIEventType eventType, GameObject gameObject)
        {
                @event._eventType = eventType;
                @event._gameObject = gameObject;
                GameEventsManager.TriggerEvent(@event);
        }
}
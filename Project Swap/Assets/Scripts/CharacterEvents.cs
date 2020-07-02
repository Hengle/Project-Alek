using JetBrains.Annotations;
using UnityEngine;

public enum CEventType { CharacterTurn, ChoosingTarget, EndOfTurn, CharacterAttacking, CharacterDeath }
public struct CharacterEvents
{
        public CEventType _eventType;
        public ScriptableObject _character;

        [UsedImplicitly] public CharacterEvents(CEventType eventType, ScriptableObject character)
        {
                _eventType = eventType;
                _character = character;
        }

        private static CharacterEvents @event;

        public static void Trigger(CEventType eventType, ScriptableObject character)
        {
                @event._eventType = eventType;
                @event._character = character;
                GameEventsManager.TriggerEvent(@event);
        }
}
using JetBrains.Annotations;
using UnityEngine;

public enum CEventType
{
        CharacterTurn,
        EnemyTurn,
        ChoosingTarget,
        EndOfTurn,
        CharacterAttacking,
        CharacterDeath,
        StatChange,
        CantPerformAction,
        NewCommand,
        SkipTurn,
        APConversion,
}

public struct CharacterEvents
{
        public CEventType _eventType;
        public ScriptableObject _character;
        public ScriptableObject _object;

        [UsedImplicitly] public CharacterEvents(CEventType eventType, ScriptableObject character, ScriptableObject @object)
        {
                _eventType = eventType;
                _character = character;
                _object = @object;
        }

        private static CharacterEvents @event;

        public static void Trigger(CEventType eventType, ScriptableObject character)
        {
                @event._eventType = eventType;
                @event._character = character;
                GameEventsManager.TriggerEvent(@event);
        }

        public static void Trigger(CEventType eventType, ScriptableObject character, ScriptableObject @object)
        {
                @event._eventType = eventType;
                @event._character = character;
                @event._object = @object;
        }
}
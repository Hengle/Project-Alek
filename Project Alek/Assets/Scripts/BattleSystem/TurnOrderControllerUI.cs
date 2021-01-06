using System.Collections.Generic;
using Characters;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class TurnOrderControllerUI : MonoBehaviour, IGameEventListener<BattleEvents>, IGameEventListener<CharacterEvents>
    {
        [SerializeField] private List<GameObject> thisTurnList = new List<GameObject>();
        [SerializeField] private List<GameObject> nextTurnList = new List<GameObject>();
        [SerializeField] private List<UnitBase> combatants = new List<UnitBase>();

        private void Start()
        {
            GameEventsManager.AddListener<BattleEvents>(this);
            GameEventsManager.AddListener<CharacterEvents>(this);
        }

        private void OnDisable()
        {
            GameEventsManager.RemoveListener<BattleEvents>(this);
            GameEventsManager.RemoveListener<CharacterEvents>(this);
        }

        private void Setup()
        {
            combatants = BattleManager._membersAndEnemies;
        }
        
        private void OnThisTurnListCreated()
        {
            var count = BattleManager._membersAndEnemiesThisTurn.Count;

            for (var i = 0; i < thisTurnList.Count; i++)
            {
                if (i >= count)
                {
                    thisTurnList[i].SetActive(false);
                    continue;
                }

                if (BattleManager._membersAndEnemiesThisTurn[i].Unit.hasPerformedTurn)
                {
                    thisTurnList[i].SetActive(false);
                    continue;
                }

                thisTurnList[i].transform.Find("Icon").
                    GetComponent<Image>().sprite = BattleManager._membersAndEnemiesThisTurn[i].icon;
                
                thisTurnList[i].SetActive(true);
            }
        }

        private void OnNextTurnListCreated()
        {
            var count = BattleManager._membersAndEnemiesNextTurn.Count;

            for (var i = 0; i < nextTurnList.Count; i++)
            {
                if (i >= count)
                {
                    nextTurnList[i].SetActive(false);
                    continue;
                }
                
                nextTurnList[i].transform.Find("Icon").
                    GetComponent<Image>().sprite = BattleManager._membersAndEnemiesNextTurn[i].icon;
                
                nextTurnList[i].SetActive(true);
            }
        }

        private void UpdateThisTurnList()
        {
            foreach (var t in thisTurnList)
            {
                if (!t.activeInHierarchy) continue;
                t.SetActive(false);
                break;
            }
        }

        public void OnGameEvent(BattleEvents eventType)
        {
            switch (eventType._battleEventType)
            {
                case BattleEventType.SetupComplete: Setup();
                    break;
                case BattleEventType.ThisTurnListCreated: OnThisTurnListCreated();
                    break;
                case BattleEventType.NextTurnListCreated: OnNextTurnListCreated();
                    break;
            }
        }

        public void OnGameEvent(CharacterEvents eventType)
        {
            switch (eventType._eventType)
            {
                case CEventType.CharacterDeath: OnThisTurnListCreated();
                    break;
                case CEventType.EndOfTurn: UpdateThisTurnList();
                    break;
                case CEventType.SkipTurn: UpdateThisTurnList();
                    break;
            }
        }
    }
}

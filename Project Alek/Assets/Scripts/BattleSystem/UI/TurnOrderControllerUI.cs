using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem.UI
{
    public class TurnOrderControllerUI : MonoBehaviour, IGameEventListener<BattleEvents>, IGameEventListener<CharacterEvents>
    {
        [SerializeField] private List<GameObject> thisTurnList = new List<GameObject>();
        [SerializeField] private List<GameObject> nextTurnList = new List<GameObject>();
        [SerializeField] private List<Image> thisTurnIcons = new List<Image>();
        [SerializeField] private List<Image> nextTurnIcons = new List<Image>();

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

        private void OnThisTurnListCreated()
        {
            var count = BattleManager.Instance.membersAndEnemiesThisTurn.Count;

            for (var i = 0; i < thisTurnList.Count; i++)
            {
                if (i >= count) { thisTurnList[i].SetActive(false); continue; }

                thisTurnIcons[i].sprite = BattleManager.Instance.membersAndEnemiesThisTurn[i].icon;
                thisTurnList[i].SetActive(true);
            }
        }

        private void OnNextTurnListCreated()
        {
            var count = BattleManager.Instance.membersAndEnemiesNextTurn.Count;

            for (var i = 0; i < nextTurnList.Count; i++)
            {
                if (i >= count) { nextTurnList[i].SetActive(false); continue; }
                
                nextTurnIcons[i].sprite = BattleManager.Instance.membersAndEnemiesNextTurn[i].icon;
                nextTurnList[i].SetActive(true);
            }
        }

        private void UpdateThisTurnList()
        {
            foreach (var t in thisTurnList.
                Where(t => t.activeInHierarchy))
            {
                t.SetActive(false);
                break;
            }
        }

        public void OnGameEvent(BattleEvents eventType)
        {
            switch (eventType._battleEventType)
            {
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
                case CEventType.EndOfTurn: UpdateThisTurnList();
                    break;
                case CEventType.SkipTurn: UpdateThisTurnList();
                    break;
            }
        }
    }
}

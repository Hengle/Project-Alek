using System.Collections.Generic;
using System.Linq;
using Characters;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem.UI
{
    public class TurnOrderControllerUI : MonoBehaviour
    {
        [SerializeField] private List<GameObject> thisTurnList = new List<GameObject>();
        [SerializeField] private List<GameObject> nextTurnList = new List<GameObject>();
        [SerializeField] private List<Image> thisTurnIcons = new List<Image>();
        [SerializeField] private List<Image> nextTurnIcons = new List<Image>();
        
        public void OnThisTurnListCreated()
        {
            var count = BattleEngine.Instance.membersAndEnemiesThisTurn.Count;
   
            for (var i = 0; i < thisTurnList.Count; i++)
            {
                if (i >= count) { thisTurnList[i].SetActive(false); continue; }

                thisTurnIcons[i].sprite = BattleEngine.Instance.membersAndEnemiesThisTurn[i].icon;
                thisTurnList[i].SetActive(true);
            }
        }

        public void OnNextTurnListCreated()
        {
            var count = BattleEngine.Instance.membersAndEnemiesNextTurn.Count;

            for (var i = 0; i < nextTurnList.Count; i++)
            {
                if (i >= count) { nextTurnList[i].SetActive(false); continue; }
                
                nextTurnIcons[i].sprite = BattleEngine.Instance.membersAndEnemiesNextTurn[i].icon;
                nextTurnList[i].SetActive(true);
            }
        }

        public void UpdateThisTurnList()
        {
            foreach (var t in thisTurnList.
                Where(t => t.activeInHierarchy))
            {
                t.SetActive(false);
                break;
            }
        }
    }
}

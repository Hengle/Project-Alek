using System;
using System.Collections.Generic;
using Characters;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class SelectableObjectManager : MonoBehaviour, IGameEventListener<CharacterEvents>
    {
        [ShowInInspector] public static List<GameObject> _enemySelectable = new List<GameObject>();
        [ShowInInspector] public static List<GameObject> _memberSelectable = new List<GameObject>();
        [ShowInInspector] public static GameObject _memberFirstSelected;
        private static bool update;

        private void Start()
        {
            GameEventsManager.AddListener(this);
            update = false;
        }

        private void OnDisable()
        {
            GameEventsManager.RemoveListener(this);
        }

        public static void SetPartySelectables()
        {
            for (var i = 0; i < _memberSelectable.Count; i++)
            {
                var unit = _memberSelectable[i].GetComponent<Unit>();
                var nav = unit.GetComponent<Selectable>().navigation;

                nav.selectOnDown = i + 1 < _memberSelectable.Count?
                    _memberSelectable[i + 1].gameObject.GetComponent<Button>() :
                    _memberSelectable[0].gameObject.GetComponent<Button>();
            
                if (i - 1 >= 0) nav.selectOnUp = _memberSelectable[i - 1].gameObject.GetComponent<Button>();
                else if (i == 0) nav.selectOnUp = _memberSelectable[_memberSelectable.Count-1].gameObject.GetComponent<Button>();

                nav.selectOnRight = _enemySelectable[i] != null ?
                    _enemySelectable[i].gameObject.GetComponent<Button>() :
                    _enemySelectable[_enemySelectable.Count-1].gameObject.GetComponent<Button>();

                unit.button.navigation = nav;
                if (i == 0) _memberFirstSelected = _memberSelectable[0].gameObject;
            }
        }

        public static void SetEnemySelectables()
        {
            for (var i = 0; i < _enemySelectable.Count; i++)
            {
                var unit = _enemySelectable[i].GetComponent<Unit>();
                var nav = unit.GetComponent<Selectable>().navigation;

                nav.selectOnDown = i + 1 < _enemySelectable.Count?
                    _enemySelectable[i + 1].gameObject.GetComponent<Button>() :
                    _enemySelectable[0].gameObject.GetComponent<Button>();
            
                if (i - 1 >= 0) nav.selectOnUp = _enemySelectable[i - 1].gameObject.GetComponent<Button>();
                else if (i == 0) nav.selectOnUp = _enemySelectable[_enemySelectable.Count-1].gameObject.GetComponent<Button>();
                
                nav.selectOnLeft = i < _memberSelectable.Count ?
                    _memberSelectable[i].gameObject.GetComponent<Button>() :
                    _memberSelectable[_memberSelectable.Count-1].gameObject.GetComponent<Button>();

                unit.button.navigation = nav;
            }
        }
        
        private static void UpdateEnemySelectables(UnitBase enemy)
        {
            Logger.Log("Updating selectables!...");
            //var found = false;
            
            _enemySelectable.Remove(enemy.Unit.gameObject);
            //Destroy(enemy.Unit.gameObject);
            
            Logger.Log("Removed! Count: " + _enemySelectable.Count);
            
            for (var i = 0; i < _enemySelectable.Count; i++)
            {
                // if (_enemySelectable[i] != enemy.Unit.gameObject) continue;
                //     
                // found = true;
                // var prevElement = i == 0 ? _enemySelectable.Count - 1 : i - 1;
                // var nextElement = i == _enemySelectable.Count - 1 ? 0 : i + 1;
                //
                // if (prevElement == nextElement) return;
                //
                // var button1 = _enemySelectable[prevElement].GetComponent<Selectable>();
                // var nav1 = button1.navigation;
                //         
                // var button2 = _enemySelectable[nextElement].GetComponent<Selectable>();
                // var nav2 = button2.navigation;
                //
                // nav1.selectOnDown = _enemySelectable[nextElement].GetComponent<Selectable>();
                // nav2.selectOnUp = _enemySelectable[prevElement].GetComponent<Selectable>();
                //
                // button1.navigation = nav1;
                // button2.navigation = nav2;
                
                var button = _enemySelectable[i].GetComponent<Selectable>();
                var nav = button.navigation;

                if (_enemySelectable.Contains(nav.selectOnUp.gameObject) &&
                    _enemySelectable.Contains(nav.selectOnDown.gameObject)) continue;
                
                Logger.Log($"Found yah {_memberSelectable[i].gameObject.name}");

                nav.selectOnUp = i > 0 ?
                    _enemySelectable[i - 1].GetComponent<Selectable>() :
                    _enemySelectable[_enemySelectable.Count - 1].GetComponent<Selectable>();
                
                nav.selectOnDown = i < _enemySelectable.Count-1 ?
                    _enemySelectable[i + 1].GetComponent<Selectable>() :
                    _enemySelectable[0].GetComponent<Selectable>();
                
                // nav.selectOnUp = i < _enemySelectable.Count ?
                //     _enemySelectable[i].GetComponent<Selectable>() :
                //     _enemySelectable[_enemySelectable.Count - 1].GetComponent<Selectable>();

                button.navigation = nav;
            }
            //if (found) _enemySelectable.Remove(enemy.Unit.gameObject);
            
            
            UpdatePartySelectables();
        }
        
        private static void UpdatePartySelectables()
        {
            for (var i = 0; i < _memberSelectable.Count; i++)
            {
                var button = _memberSelectable[i].GetComponent<Selectable>();
                var nav = button.navigation;

                if (_enemySelectable.Contains(nav.selectOnRight.gameObject)) continue;
                
                Logger.Log($"Found yah {_memberSelectable[i].gameObject.name}");
                
                nav.selectOnRight = i < _enemySelectable.Count ?
                    _enemySelectable[i].GetComponent<Selectable>() :
                    _enemySelectable[_enemySelectable.Count - 1].GetComponent<Selectable>();

                button.navigation = nav;
            }
        }

        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType == CEventType.CharacterDeath && eventType._character as Enemy)
            {
                UpdateEnemySelectables((UnitBase)eventType._character);
            }
        }
    }
}
using System.Collections.Generic;
using Characters;
using Characters.Enemies;
using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class SelectableObjectManager : MonoBehaviour, IGameEventListener<UnitBase,CharacterGameEvent>
    {
        [SerializeField] private CharacterGameEvent characterDeathEvent;
        [ShowInInspector] public static List<Selectable> _enemySelectable = new List<Selectable>();
        [ShowInInspector] public static List<Selectable> _memberSelectable = new List<Selectable>();
        [ShowInInspector] public static GameObject _memberFirstSelected;

        private void Start() => characterDeathEvent.AddListener(this);
        private void OnDisable() => characterDeathEvent.RemoveListener(this);
        
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

                nav.selectOnRight = i < _enemySelectable.Count ?
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
            _enemySelectable.Remove(enemy.Selectable);

            for (var i = 0; i < _enemySelectable.Count; i++)
            {
                var button = _enemySelectable[i];
                var nav = button.navigation;

                if (_enemySelectable.Contains(nav.selectOnUp) &&
                    _enemySelectable.Contains(nav.selectOnDown)) continue;

                nav.selectOnUp = i > 0 ? _enemySelectable[i - 1] :
                    _enemySelectable[_enemySelectable.Count - 1];
                
                nav.selectOnDown = i < _enemySelectable.Count-1 ?
                    _enemySelectable[i + 1] :
                    _enemySelectable[0];

                button.navigation = nav;
            }
            
            UpdatePartySelectables();
        }
        
        private static void UpdatePartySelectables()
        {
            for (var i = 0; i < _memberSelectable.Count; i++)
            {
                var button = _memberSelectable[i];
                var nav = button.navigation;

                if (_enemySelectable.Contains(nav.selectOnRight)) continue;

                nav.selectOnRight = i < _enemySelectable.Count ? _enemySelectable[i] :
                    _enemySelectable[_enemySelectable.Count - 1];

                button.navigation = nav;
            }
        }

        public void OnEventRaised(UnitBase value1, CharacterGameEvent value2)
        {
            if (value2 != characterDeathEvent || !(value1 as Enemy)) return;
            if (_enemySelectable.Count == 1) return;
            
            UpdateEnemySelectables(value1);
        }
    }
}
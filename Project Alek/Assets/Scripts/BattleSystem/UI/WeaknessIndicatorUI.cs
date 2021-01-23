using System.Linq;
using BattleSystem.Generator;
using Characters;
using Characters.PartyMembers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BattleSystem.UI
{
    public class WeaknessIndicatorUI : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        private BattleGeneratorDatabase database;
        private GameObject indicator;
        private Unit unit;

        private bool IsWeakToAbility
        {
            get 
            {
                var ability = BattleEngine.Instance.activeUnit.Unit.currentAbility;
                return ability != null && ability.hasElemental && unit.parent._elementalWeaknesses.
                    Any(kvp => kvp.Key._type == ability.elementalType && kvp.Value);
            }
        }

        //TODO: Have to account for spells, which don't use weapon damage types
        private bool IsWeakToDamageType => unit.parent._damageTypeWeaknesses.Any(type => 
                type.Key == ((PartyMember) BattleEngine.Instance.activeUnit).equippedWeapon.damageType && type.Value);

        private void Start()
        {
            database = FindObjectOfType<BattleGeneratorDatabase>();
            unit = GetComponent<Unit>();

            unit.onSelect += OnMultiSelect;
            unit.onDeselect += OnMultiDeselect;
            
            InstantiatePrefab();
        }

        private void InstantiatePrefab()
        {
            var position = gameObject.transform.position;
            var newPosition = new Vector3(position.x, position.y + 1.75f, position.z);

            indicator = Instantiate(database.weaknessIndicator, newPosition,
                database.weaknessIndicator.transform.rotation);
            
            indicator.transform.SetParent(gameObject.transform);
            indicator.SetActive(false);
        }

        private bool IsWeak() => IsWeakToAbility || IsWeakToDamageType;
        
        private void OnMultiSelect() => indicator.SetActive(IsWeak());
        
        private void OnMultiDeselect() => indicator.SetActive(false);
        
        public void OnSelect(BaseEventData eventData) => indicator.SetActive(IsWeak());

        public void OnDeselect(BaseEventData eventData) => indicator.SetActive(false);
        
        private void OnDisable()
        {
            if (!unit) return;
            unit.onSelect -= OnMultiSelect;
            unit.onDeselect -= OnMultiDeselect;
        }
    }
}

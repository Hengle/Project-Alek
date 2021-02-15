using System.Collections.Generic;
using Audio;
using Characters;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BattleSystem.UI
{
    public class APConversionControllerUI : MonoBehaviour, IGameEventListener<UnitBase,CharacterGameEvent>
    {
        public Unit unit;
        [SerializeField] private CharacterGameEvent conversionEvent;
        [SerializeField] private CharacterGameEvent chooseTargetEvent;
        [SerializeField] private List<GameObject> arrows;

        private Vector3 ArrowPosition
        {
            get
            {
                if (!EventSystem.current.currentSelectedGameObject) return transform.localPosition;
                var position = EventSystem.current.currentSelectedGameObject.transform.localPosition;
                var newPosition = new Vector3(position.x + 325, position.y, position.z);
                return newPosition;
            }
        }

        private void OnEnable()
        {
            conversionEvent.AddListener(this);
            chooseTargetEvent.AddListener(this);
        }

        private void OnDisable()
        {
            conversionEvent.RemoveListener(this);
            chooseTargetEvent.RemoveListener(this);
        }

        private void AdjustConversionLevel()
        {
            for (var i = 0; i < arrows.Count; i++)
            {
                if (i >= unit.conversionLevel) { arrows[i].SetActive(false); continue; }
                
                AudioController.Instance.PlayAudio(CommonAudioTypes.Instance.aPConversion);
                arrows[i].SetActive(true);
            }
        }

        private void DeactivateArrows() => arrows.ForEach(a => a.gameObject.SetActive(false));

        private void SetArrowPosition() => transform.localPosition = ArrowPosition;

        public void OnEventRaised(UnitBase value1, CharacterGameEvent value2)
        {
            if (value1 == unit.parent && value2 == conversionEvent)
            {
                SetArrowPosition();
                AdjustConversionLevel();
            }
            else if (value1 == unit.parent && value2 == chooseTargetEvent)
            {
                DeactivateArrows();
            }
        }
    }
}

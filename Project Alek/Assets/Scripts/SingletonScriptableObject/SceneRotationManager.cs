using System.Collections;
using System.Globalization;
using Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SingletonScriptableObject
{
    [CreateAssetMenu(fileName = "Scene Rotation Manager", menuName = "Singleton SO/Scene Rotation Manager")]
    public class SceneRotationManager : ScriptableObjectSingleton<SceneRotationManager>
    {
        [SerializeField] [ValueDropdown(nameof(GetAllRotationOptionsInScene), IsUniqueList = true)]
        private Quaternion currentRotation = Quaternion.identity;
        
        public Quaternion CurrentRotation
        {
            get => currentRotation;
            set => currentRotation = value;
        }

        private IEnumerable GetAllRotationOptionsInScene()
        {
            var rotationTriggers = FindObjectsOfType<CameraRotationTrigger>();
            if (rotationTriggers.Length == 0) yield break;

            foreach (var trigger in rotationTriggers)
            {
                yield return new ValueDropdownItem(trigger.ExitRotation.eulerAngles.y.
                    ToString(CultureInfo.InvariantCulture), trigger.ExitRotation);
                
                yield return new ValueDropdownItem(trigger.AreaRotation.eulerAngles.y.
                    ToString(CultureInfo.InvariantCulture), trigger.AreaRotation);
            }
            
            yield return new ValueDropdownItem("Identity", Quaternion.identity);
        }

        [Button] private void ResetRotation() => CurrentRotation = Quaternion.identity;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}
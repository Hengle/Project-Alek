using BattleSystem;
using SceneManagement;
using UnityEngine;

namespace Characters
{
    public class TutorialKingNpc : MonoBehaviour
    {
        [SerializeField] private BattleEngine tutorialEngine;
        
        public void StartTutorialBattle()
        {
            Battle.OverrideEngine(tutorialEngine);
            SceneLoadManager.LoadBattle();
        }
    }
}
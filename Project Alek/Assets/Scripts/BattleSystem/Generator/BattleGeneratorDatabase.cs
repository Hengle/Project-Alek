using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using UnityEngine;

namespace BattleSystem.Generator
{
    public class BattleGeneratorDatabase : MonoBehaviour
    {
        public List<GameObject> inventoryCanvases = new List<GameObject>();
        public List<GameObject> closeUpCameras = new List<GameObject>();
        public List<GameObject> criticalCameras = new List<GameObject>();
        public List<GameObject> characterPanels = new List<GameObject>();
        public List<GameObject> characterSpawnPoints = new List<GameObject>();
        public List<GameObject> enemySpawnPoints = new List<GameObject>();
        public BattleOptionsPanel boPanel;
        public GameObject battlePanelGO;
        public Transform battlePanelSpawnPoint;
        public Transform enemyStatusBox;
        public Transform profileBox;
        public Transform shieldTransform;
        public Canvas worldSpaceCanvas;
        public GameObject weaknessIndicator;
    }
}
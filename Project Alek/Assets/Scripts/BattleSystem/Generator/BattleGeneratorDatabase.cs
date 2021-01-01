using System.Collections.Generic;
using Characters;
using MoreMountains.InventoryEngine;
using UnityEngine;

namespace BattleSystem.Generator
{
    // For random encounters, consider separating enemy list into own script and making it an instance like party members
    // Consider making this a scriptable object
    public class BattleGeneratorDatabase : MonoBehaviour
    {
        public List<InventoryItem> inventoryItems = new List<InventoryItem>();
        public List<GameObject> inventoryCanvases = new List<GameObject>();
        public List<GameObject> closeUpCameras = new List<GameObject>();
        public List<GameObject> criticalCameras = new List<GameObject>();
        public List<GameObject> characterPanels = new List<GameObject>();
        public List<GameObject> characterSpawnPoints = new List<GameObject>();
        public List<GameObject> enemySpawnPoints = new List<GameObject>();
        public List<Enemy> enemies = new List<Enemy>();
        public BattleOptionsPanel boPanel;
        public Transform enemyStatusBox;
        public Transform profileBox;
        public Transform shieldTransform;
    }
}
﻿using System.Collections.Generic;
using Characters;
using Characters.PartyMembers;
using UnityEngine;

namespace BattleSystem.Generator
{
    // For random encounters, consider separating enemy list into own script and making it an instance like party members
    // Consider making this a script object
    public class BattleGeneratorDatabase : MonoBehaviour
    {
        public List<GameObject> characterPanels = new List<GameObject>();
        public List<GameObject> characterSpawnPoints = new List<GameObject>();
        public List<GameObject> enemySpawnPoints = new List<GameObject>();
        public List<Enemy> enemies = new List<Enemy>();
        public BattleOptionsPanel boPanel;
        public Transform statusBox;
        public Transform profileBox;
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Characters.Enemies
{
    public class EnemyManager : MonoBehaviour
    {
        public List<Enemy> enemies = new List<Enemy>();

        public static EnemyManager _instance;

        private void Awake()
        {
            if (_instance == null) {
                DontDestroyOnLoad(gameObject);
                _instance = this;
            }
            
            else if (_instance != this) Destroy(gameObject);
        }
    }
}

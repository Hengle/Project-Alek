using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters.Enemies;
using UnityEngine;

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

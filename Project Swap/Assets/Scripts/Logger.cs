﻿using UnityEngine;

public class Logger : MonoBehaviour
{
        [SerializeField] private bool showLogs = true;
        private static bool displayLogs;
        private void Update() => displayLogs = showLogs;

        public static void Log(string message) {
                if (displayLogs) Debug.Log(message);
        }
}
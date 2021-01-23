using System;
using UnityEngine;

namespace Overworld
{
    public abstract class Spawnable : MonoBehaviour
    {
        public Action OnDestroyAction { get; set; }
        
        protected virtual void OnDestroy() { OnDestroyAction?.Invoke(); }
    }
}
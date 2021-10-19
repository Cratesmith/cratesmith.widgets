using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace com.cratesmith.widgets
{
    public static class GameObjectExtensions
    {
        public static bool EnsureComponent<TSelf, TComponent>(this TSelf _self, ref TComponent _component) 
            where TSelf:MonoBehaviour
            where TComponent:Component
        {
            if (_component) return true;
            _component = _self.GetComponent<TComponent>();
            if (_component) return true;
            _component = _self.gameObject.AddComponent<TComponent>();
            Assert.IsNotNull(_component);
            return false;
        }
    }
}
// ReSharper disable Unity.PerformanceAnalysis

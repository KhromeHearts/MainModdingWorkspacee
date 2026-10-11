using UnityEngine;

namespace Optime {
    internal static class GameObjectExtension {
        public static void InitializeFeature<T>(this GameObject gameObject, bool loaded = true) where T : Feature<T> {
            gameObject.AddComponent<T>();
            Feature<T>.SetLoaded(loaded);
        }
    }
}
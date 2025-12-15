using UnityEngine;

namespace DefaultNamespace.ExtensionMethods
{
    public static class GameObjectExtensions
    {
        public static bool Is<T>(this GameObject gameObject) where T : Component {
            return gameObject.GetComponent<T>() != null;   
        } 
        
        public static T As<T>(this GameObject gameObject) where T : Component {
            return gameObject.GetComponent<T>();   
        }
    }
}
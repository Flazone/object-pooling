#if UNITY_EDITOR
#define VERBOSE // Enable/disable pools instances renaming (useful to keep a clean hierarchy but generates a lot of garbage) 
#endif

using UnityEngine;

namespace FLZ.Pooling
{
    public abstract class UnityObjectPool<T> : AbstractPool<T> where T : UnityEngine.Component, IPoolable
    {
        protected Transform objectsParentTransform;
        
        protected UnityObjectPool(int size, IFactory<T> factory, Transform parent) : base(size, factory)
        {
            objectsParentTransform = new GameObjectFactory().Create().transform;
            objectsParentTransform.SetParent(parent, true);
            
#if UNITY_EDITOR
            // We want to keep the hierarchy clean and readable in the editor, but it slows down the pool and we dont need it on build :)
            SetInstancesInParent();
#endif
        }
        
        protected override void Grow()
        {
            base.Grow();
            
            UpdatePoolName();
            Debug.LogWarning($"Refilled stack. New size = {poolSize}", objectsParentTransform);
        }

        protected override void OnPoolDepleted()
        {
            Debug.LogError($"Pool {objectsParentTransform.name} capacity [{poolSize}] reached");
        }
        
        protected virtual void UpdatePoolName() { }

        private void SetInstancesInParent()
        {
            UpdatePoolName();
            foreach (var poolObject in stack)
            {
                poolObject.transform.SetParent(objectsParentTransform);
            }
        }
    }

    /// <summary>
    /// MonoBehaviour pool implementation, can spawn any object inheriting MonoBehaviour
    /// ie: Prefabs, components...
    /// </summary>
    public class MonoPool<T> : UnityObjectPool<T> where T : MonoBehaviour, IPoolable
    {
        private string prefabName = null;

        /// <summary>
        /// Initialize and fill the pool by instantiating objects using the given factory.
        /// </summary>
        /// <param name="size">The amount of objects instantiated in the pool.</param>
        /// <param name="factory">The factory in charge of creating pool's objects</param>
        /// <param name="parent">The parent transform of the objects pool.</param>
        public MonoPool(int size, IFactory<T> factory, Transform parent) : base(size, factory, parent) { }
        
        /// <summary>
        /// Initialize and fill the pool by instantiating objects using a PrefabFactory filled with the given prefab.
        /// </summary>
        /// <param name="size">The amount of objects instantiated in the pool.</param>
        /// <param name="prefab">The prefab gave to the PrefabFactory</param>
        /// <param name="parent">The parent transform of the objects pool.</param>
        public MonoPool(int size, GameObject prefab, Transform parent) : base(size, new PrefabFactory<T>(prefab), parent) { }

        protected override T CreateObject()
        {
            T theObject = base.CreateObject();
            theObject.transform.SetParent(objectsParentTransform, false);

#if VERBOSE
            if (string.IsNullOrEmpty(prefabName))
            {
                prefabName = theObject.name.Replace("(Clone)", "");
            }

            theObject.name = prefabName;
#endif
            theObject.gameObject.SetActive(false);
            theObject.transform.position = Vector3.zero;

            return theObject;
        }

        public override T Spawn()
        {
            return Spawn(Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Spawn a new GameObject with the given position/rotation/parent and set it active.
        /// </summary>
        public T Spawn(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            T thePooled = base.Spawn();
            if (thePooled != null)
            {
#if VERBOSE
                thePooled.name += " (In use)";
#endif

                if (parent)
                {
                    thePooled.transform.SetParent(parent);
                }

                thePooled.transform.SetPositionAndRotation(position, rotation);
                thePooled.gameObject.SetActive(true);
            }

            return thePooled;
        }

        /// <summary>
        /// UnSpawns a given GameObject, de-activate it and adds it back to the pool.
        /// </summary>
        public override void DeSpawn(T thePooled)
        {
            var pooled = thePooled as T;
#if VERBOSE
            pooled.name = pooled.name.Replace(" (In use)", "");
#endif
#if UNITY_EDITOR
            pooled.transform.SetParent(objectsParentTransform);
#endif
            pooled.gameObject.SetActive(false);

            base.DeSpawn(thePooled);
        }

        protected override void UpdatePoolName()
        {
#if VERBOSE
             objectsParentTransform.name = $"{prefabName} pool ({poolSize})";
#endif
        }
    }
}
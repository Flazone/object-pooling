using UnityEngine;

namespace FLZ.Pooling.Examples
{
    public abstract class PooledBehaviour : MonoBehaviour, IPoolable
    {
        public MonoPool<PooledBehaviour> Pool;

        #region IPoolable
        public void OnSpawn() { }

        public virtual void OnDeSpawn() { }
        #endregion
    }
}
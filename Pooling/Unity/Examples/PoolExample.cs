using UnityEngine;
using Random = UnityEngine.Random;

namespace FLZ.Pooling.Examples
{
    public class PoolExample : MonoBehaviour
    {
        public GameObject PooledPrefab;
        public AudioClip Clip;

        private MonoPool<PooledBehaviour> gameObjectPool;
        private MonoPool<PoolableAudioSource> audioSourcePool;

        private void Awake()
        {
            gameObjectPool = new MonoPool<PooledBehaviour>(10, PooledPrefab, transform);
            audioSourcePool = new MonoPool<PoolableAudioSource>(10, new ComponentFactory<PoolableAudioSource>(), transform);
        }

        public void SpawnVFX()
        {
            var position = Random.insideUnitCircle * 5.0f;
            
            PooledBehaviour vfx = gameObjectPool.Spawn(position, Quaternion.identity);
            vfx.Pool = gameObjectPool;

            PoolableAudioSource pooledAudioSource = audioSourcePool.Spawn(position, Quaternion.identity);
            pooledAudioSource.Pool = audioSourcePool;
            pooledAudioSource.SetupAndPlay(Clip);
        }
    }
}
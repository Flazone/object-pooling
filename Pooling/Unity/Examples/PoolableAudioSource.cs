using System.Collections;
using UnityEngine;

namespace FLZ.Pooling.Examples
{
    /// <summary>
    /// Very simple example of a pooled audio source with custom OnSpawn and DeSpawn setup
    ///
    /// Of course, the playing and despawn logic should be moved into a proper SoundManager 
    /// </summary>
    public class PoolableAudioSource : MonoBehaviour, IPoolable
    {
        public MonoPool<PoolableAudioSource> Pool;
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = new ComponentFactory<AudioSource>().Create(transform);
        }

        public void SetupAndPlay(AudioClip clip)
        {
            audioSource.clip = clip;
            audioSource.Play();

            StartCoroutine(WaitForClipEnd());
        }

        private IEnumerator WaitForClipEnd()
        {
            yield return new WaitForSeconds(audioSource.clip.length);
            Pool.DeSpawn(this);
        }

        #region IPoolable
        public void OnSpawn()
        {
#if UNITY_EDITOR
            gameObject.name += " (Playing)";
#endif
            audioSource.loop = false;
            audioSource.volume = 1f;
            audioSource.pitch = 1f;
            audioSource.panStereo = 0f;
            audioSource.time = 0f;
        }

        public void OnDeSpawn()
        {
#if UNITY_EDITOR
            gameObject.name = gameObject.name.Replace(" (Playing)", "");
#endif
            audioSource.Stop();
            audioSource.clip = null;
        }

        #endregion
    }
}
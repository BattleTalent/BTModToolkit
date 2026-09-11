using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossLink
{
    public class SoundEffectPlayer : MonoBehaviour
    {
        public SoundEffectInfo soundInfo;
        public bool loop = false;

#if UNITY_EDITOR
        private AudioSource previewAudioSource;

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                PlaySound();
            }
        }

        private void OnDisable()
        {
            StopSound();
        }

        public void PlaySound()
        {
            StopSound();

            if (soundInfo == null || soundInfo.soundNames == null || soundInfo.soundNames.Length == 0)
            {
                return;
            }

            string soundName = soundInfo.soundNames[Random.Range(0, soundInfo.soundNames.Length)];
            const string soundAddressPrefix = "Audio/Sound/";
            string soundAddress = soundName.StartsWith(soundAddressPrefix)
                ? soundName
                : soundAddressPrefix + soundName;
            string assetPath = AddressableHelper.GetAddressableAssetPath(soundAddress);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning($"Cannot preview sound '{soundName}': no matching Addressable audio asset was found.", this);
                return;
            }

            AudioClip clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (clip == null)
            {
                Debug.LogWarning($"Cannot preview sound '{soundName}': '{assetPath}' is not an AudioClip.", this);
                return;
            }

            if (previewAudioSource == null)
            {
                previewAudioSource = gameObject.AddComponent<AudioSource>();
            }

            previewAudioSource.clip = clip;
            previewAudioSource.loop = loop;
            previewAudioSource.spatialBlend = 0f;
            previewAudioSource.volume = soundInfo.vol + Random.Range(-soundInfo.volRandomRange, soundInfo.volRandomRange);
            previewAudioSource.pitch = soundInfo.pitchMax > 0f
                ? Random.Range(soundInfo.pitchMin, soundInfo.pitchMax)
                : 1f;
            previewAudioSource.Play();
        }

        private void StopSound()
        {
            if (previewAudioSource != null)
            {
                previewAudioSource.Stop();
            }
        }
#else
        public void PlaySound()
        {
        }
#endif
    }

}

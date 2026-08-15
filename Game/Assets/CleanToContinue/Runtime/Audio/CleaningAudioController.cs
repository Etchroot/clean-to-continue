using System.Collections;
using CleanToContinue.Core;
using CleanToContinue.Input;
using UnityEngine;

namespace CleanToContinue.Audio
{
    public sealed class CleaningAudioController : MonoBehaviour, IContinuousToolAudio
    {
        [SerializeField] private AudioSource airGunSource;
        [SerializeField] private AudioSource cottonSwabSource;
        [SerializeField] private AudioSource clothSource;
        [SerializeField] private AudioSource completionSource;
        [SerializeField, Min(0.01f)] private float crossFadeSeconds = 0.08f;

        private PrototypeAudioFactory.ClipSet clips;
        private Coroutine fadeRoutine;
        private float sfxVolume = 1f;

        public void NotifyUserInteraction()
        {
            EnsurePrototypeAudio();
        }

        public void SetSfxVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        public void BeginCleaning(CleaningTool selectedTool)
        {
            EnsurePrototypeAudio();
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            EnsureLoopIsPlaying(airGunSource);
            EnsureLoopIsPlaying(cottonSwabSource);
            EnsureLoopIsPlaying(clothSource);
            fadeRoutine = StartCoroutine(CrossFadeTo(selectedTool));
        }

        public void StopContinuousToolAudio()
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }

            StopSource(airGunSource);
            StopSource(cottonSwabSource);
            StopSource(clothSource);
        }

        public void PlayCompletion()
        {
            StopContinuousToolAudio();
            EnsurePrototypeAudio();
            completionSource.PlayOneShot(clips.Completion, sfxVolume);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                StopContinuousToolAudio();
            }
        }

        private void OnDisable()
        {
            StopContinuousToolAudio();
        }

        private void OnDestroy()
        {
            DestroyClip(clips?.AirGun);
            DestroyClip(clips?.CottonSwab);
            DestroyClip(clips?.Cloth);
            DestroyClip(clips?.Completion);
        }

        private void EnsurePrototypeAudio()
        {
            if (clips != null)
            {
                return;
            }

            EnsureSources();
            clips = PrototypeAudioFactory.Create();
            ConfigureLoop(airGunSource, clips.AirGun);
            ConfigureLoop(cottonSwabSource, clips.CottonSwab);
            ConfigureLoop(clothSource, clips.Cloth);
            completionSource.playOnAwake = false;
            completionSource.loop = false;
        }

        private void EnsureSources()
        {
            airGunSource = airGunSource != null ? airGunSource : CreateSource("Air Gun Loop");
            cottonSwabSource = cottonSwabSource != null ? cottonSwabSource : CreateSource("Cotton Swab Loop");
            clothSource = clothSource != null ? clothSource : CreateSource("Cloth Loop");
            completionSource = completionSource != null ? completionSource : CreateSource("Completion One Shot");
        }

        private AudioSource CreateSource(string sourceName)
        {
            var sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform, false);
            return sourceObject.AddComponent<AudioSource>();
        }

        private static void ConfigureLoop(AudioSource source, AudioClip clip)
        {
            source.playOnAwake = false;
            source.loop = true;
            source.clip = clip;
            source.volume = 0f;
        }

        private static void EnsureLoopIsPlaying(AudioSource source)
        {
            if (!source.isPlaying)
            {
                source.Play();
            }
        }

        private IEnumerator CrossFadeTo(CleaningTool selectedTool)
        {
            var elapsed = 0f;
            var airStart = airGunSource.volume;
            var cottonStart = cottonSwabSource.volume;
            var clothStart = clothSource.volume;
            while (elapsed < crossFadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / crossFadeSeconds);
                airGunSource.volume = Mathf.Lerp(airStart, selectedTool == CleaningTool.AirGun ? sfxVolume : 0f, t);
                cottonSwabSource.volume = Mathf.Lerp(cottonStart, selectedTool == CleaningTool.CottonSwab ? sfxVolume : 0f, t);
                clothSource.volume = Mathf.Lerp(clothStart, selectedTool == CleaningTool.Cloth ? sfxVolume : 0f, t);
                yield return null;
            }

            airGunSource.volume = selectedTool == CleaningTool.AirGun ? sfxVolume : 0f;
            cottonSwabSource.volume = selectedTool == CleaningTool.CottonSwab ? sfxVolume : 0f;
            clothSource.volume = selectedTool == CleaningTool.Cloth ? sfxVolume : 0f;
            fadeRoutine = null;
        }

        private static void StopSource(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            source.Stop();
            source.volume = 0f;
        }

        private static void DestroyClip(AudioClip clip)
        {
            if (clip != null)
            {
                Destroy(clip);
            }
        }
    }
}

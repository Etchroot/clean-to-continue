using System;
using UnityEngine;

namespace CleanToContinue.Audio
{
    public static class PrototypeAudioFactory
    {
        public const int SampleRate = 44100;
        public const int DefaultNoiseSeed = 240816;

        public sealed class ClipSet
        {
            public AudioClip AirGun;
            public AudioClip CottonSwab;
            public AudioClip Cloth;
            public AudioClip Completion;
        }

        public static ClipSet Create(int seed = DefaultNoiseSeed)
        {
            return new ClipSet
            {
                AirGun = CreateNoiseLoop("Prototype Air Gun", seed, NoiseKind.FilteredAir),
                CottonSwab = CreateNoiseLoop("Prototype Cotton Swab", seed + 1, NoiseKind.HighFriction),
                Cloth = CreateNoiseLoop("Prototype Cloth", seed + 2, NoiseKind.LowFriction),
                Completion = CreateCompletionChime()
            };
        }

        private static AudioClip CreateNoiseLoop(string name, int seed, NoiseKind kind)
        {
            var samples = new float[SampleRate];
            var random = new System.Random(seed);
            var lowPass = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var noise = (float)(random.NextDouble() * 2d - 1d);
                lowPass += (noise - lowPass) * (kind == NoiseKind.LowFriction ? 0.035f : 0.16f);
                var highPass = noise - lowPass;
                var frictionPulse = 0.65f + 0.35f * Mathf.Sin(i * Mathf.PI * 2f / 173f);
                switch (kind)
                {
                    case NoiseKind.HighFriction:
                        samples[i] = highPass * frictionPulse * 0.11f;
                        break;
                    case NoiseKind.LowFriction:
                        samples[i] = lowPass * frictionPulse * 0.2f;
                        break;
                    default:
                        samples[i] = (lowPass * 0.75f + noise * 0.25f) * 0.24f;
                        break;
                }
            }

            return CreateClip(name, samples);
        }

        private static AudioClip CreateCompletionChime()
        {
            var sampleCount = Mathf.RoundToInt(SampleRate * 0.7f);
            var samples = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var time = (float)i / SampleRate;
                var frequency = time < 0.28f ? 523.25f : 659.25f;
                var noteTime = time < 0.28f ? time : time - 0.28f;
                var envelope = Mathf.Exp(-4.5f * noteTime) * Mathf.Clamp01(time * 45f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * time) * envelope * 0.28f;
            }

            return CreateClip("Prototype Completion", samples);
        }

        private static AudioClip CreateClip(string name, float[] samples)
        {
            var clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private enum NoiseKind
        {
            FilteredAir,
            HighFriction,
            LowFriction
        }
    }
}

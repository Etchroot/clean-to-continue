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
            public AudioClip Cloth;
            public AudioClip Completion;
        }

        public static ClipSet Create(int seed = DefaultNoiseSeed)
        {
            return new ClipSet
            {
                AirGun = CreateNoiseLoop("Prototype Air Gun", seed, NoiseKind.FilteredAir),
                Cloth = CreateClothSqueakLoop(seed + 1),
                Completion = CreateCompletionChime()
            };
        }

        private static AudioClip CreateClothSqueakLoop(int seed)
        {
            var samples = new float[SampleRate];
            var random = new System.Random(seed);
            var lowPass = 0f;
            var burstStarts = new[] { 0.08f, 0.31f, 0.54f, 0.77f };
            const float burstDuration = 0.085f;

            for (var i = 0; i < samples.Length; i++)
            {
                var time = (float)i / SampleRate;
                var noise = (float)(random.NextDouble() * 2d - 1d);
                lowPass += (noise - lowPass) * 0.025f;
                var value = lowPass * 0.018f;

                for (var burst = 0; burst < burstStarts.Length; burst++)
                {
                    var localTime = time - burstStarts[burst];
                    if (localTime < 0f || localTime >= burstDuration)
                    {
                        continue;
                    }

                    var normalized = localTime / burstDuration;
                    var envelope = Mathf.Sin(Mathf.PI * normalized);
                    envelope *= envelope;
                    var startFrequency = burst % 2 == 0 ? 1250f : 1050f;
                    var endFrequency = burst % 2 == 0 ? 720f : 1550f;
                    var sweepRate = (endFrequency - startFrequency) / burstDuration;
                    var phase = Mathf.PI * 2f *
                        (startFrequency * localTime + 0.5f * sweepRate * localTime * localTime);
                    var squeak = Mathf.Sin(phase) + Mathf.Sin(phase * 2.01f) * 0.18f;
                    value += squeak * envelope * 0.19f;
                }

                samples[i] = Mathf.Clamp(value, -0.24f, 0.24f);
            }

            return CreateClip("Prototype Cloth Squeak", samples);
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
            var starts = new[] { 0f, 0.18f, 0.38f };
            var frequencies = new[] { 987.77f, 1318.51f, 1760f };
            for (var i = 0; i < sampleCount; i++)
            {
                var time = (float)i / SampleRate;
                var sample = 0f;
                for (var note = 0; note < starts.Length; note++)
                {
                    var noteTime = time - starts[note];
                    if (noteTime < 0f)
                    {
                        continue;
                    }

                    var attack = Mathf.Clamp01(noteTime * 90f);
                    var envelope = attack * Mathf.Exp(-7.5f * noteTime);
                    var phase = 2f * Mathf.PI * frequencies[note] * noteTime;
                    sample += (Mathf.Sin(phase) + Mathf.Sin(phase * 2f) * 0.18f) * envelope * 0.22f;
                }

                samples[i] = Mathf.Clamp(sample, -0.8f, 0.8f);
            }

            return CreateClip("Prototype Three Note Bell", samples);
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

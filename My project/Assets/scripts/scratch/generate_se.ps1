$code = @"
using System;
using System.IO;

public static class JuwanGenerator
{
    private const int SampleRate = 44100;

    public static void GenerateSEs(string outputDir)
    {
        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

        string normalPath = Path.Combine(outputDir, "SE_EnemyShot_Touhou_Juwan.wav");
        byte[] normalWav = SynthesizeJuwanSE(false);
        File.WriteAllBytes(normalPath, normalWav);
        Console.WriteLine("Generated Normal: " + normalPath);

        string heavyPath = Path.Combine(outputDir, "SE_EnemyShot_Touhou_Juwan_Heavy.wav");
        byte[] heavyWav = SynthesizeJuwanSE(true);
        File.WriteAllBytes(heavyPath, heavyWav);
        Console.WriteLine("Generated Heavy: " + heavyPath);
    }

    private static byte[] SynthesizeJuwanSE(bool isHeavy)
    {
        float duration = isHeavy ? 0.85f : 0.45f;
        int totalSamples = (int)(SampleRate * duration);

        float[] leftBuffer = new float[totalSamples];
        float[] rightBuffer = new float[totalSamples];

        float startFreq = isHeavy ? 2000f : 2400f;
        float endFreq = isHeavy ? 180f : 320f;
        float modRatio = isHeavy ? 2.0f : 2.76f;
        float modIndexStart = isHeavy ? 6.5f : 4.2f;

        float noiseCutoffStart = isHeavy ? 4200f : 3400f;
        float noiseCutoffEnd = isHeavy ? 350f : 750f;
        float noiseResonance = isHeavy ? 6.5f : 5.0f;

        int maxDelaySamples = (int)(SampleRate * 0.02f);
        float[] delayBufferL = new float[maxDelaySamples];
        float[] delayBufferR = new float[maxDelaySamples];
        int delayIndex = 0;

        float bpfLowL = 0f, bpfBandL = 0f;
        float bpfLowR = 0f, bpfBandR = 0f;

        Random rand = new Random(1337);

        double carrierPhaseL = 0.0;
        double carrierPhaseR = 0.0;
        double modPhase = 0.0;
        double subBasePhase = 0.0;

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / SampleRate;
            float progress = t / duration;

            float env;
            float attackTime = 0.005f;
            if (t < attackTime)
            {
                env = t / attackTime;
            }
            else
            {
                float decayFactor = isHeavy ? 4.5f : 6.0f;
                env = (float)Math.Exp(-(t - attackTime) * decayFactor);
            }

            float pitchProgress = (float)Math.Pow(progress, 0.4);
            float currentFreq = startFreq + (endFreq - startFreq) * pitchProgress;
            float currentModFreq = currentFreq * modRatio;
            float currentModIndex = modIndexStart * (1f - (float)Math.Pow(progress, 0.5));

            modPhase += 2.0 * Math.PI * currentModFreq / SampleRate;
            float modSignal = (float)Math.Sin(modPhase) * currentModIndex;

            float detune = isHeavy ? 3.0f : 1.5f;
            carrierPhaseL += 2.0 * Math.PI * (currentFreq + detune) / SampleRate;
            carrierPhaseR += 2.0 * Math.PI * (currentFreq - detune) / SampleRate;

            float fmSignalL = (float)Math.Sin(carrierPhaseL + modSignal);
            float fmSignalR = (float)Math.Sin(carrierPhaseR + modSignal);

            float whiteNoiseL = (float)(rand.NextDouble() * 2.0 - 1.0);
            float whiteNoiseR = (float)(rand.NextDouble() * 2.0 - 1.0);

            float cutoff = noiseCutoffStart + (noiseCutoffEnd - noiseCutoffStart) * (float)Math.Pow(progress, 0.5);
            float f = 2f * (float)Math.Sin(Math.PI * cutoff / SampleRate);
            float q = 1f / noiseResonance;

            bpfLowL += f * bpfBandL;
            float bpfHighL = whiteNoiseL - bpfLowL - q * bpfBandL;
            bpfBandL += f * bpfHighL;

            bpfLowR += f * bpfBandR;
            float bpfHighR = whiteNoiseR - bpfLowR - q * bpfBandR;
            bpfBandR += f * bpfHighR;

            float noiseLevel = isHeavy ? 0.75f : 0.6f;
            float noiseSignalL = bpfBandL * noiseLevel;
            float noiseSignalR = bpfBandR * noiseLevel;

            float subBass = 0f;
            if (isHeavy)
            {
                float subFreq = 95f + (38f - 95f) * progress;
                subBasePhase += 2.0 * Math.PI * subFreq / SampleRate;
                float subEnv = (float)Math.Exp(-t * 5.0);
                float rawSub = (float)Math.Sin(subBasePhase);
                subBass = (float)Math.Tanh(rawSub * 1.5) * subEnv * 0.9f;
            }

            float combinedL = (fmSignalL * 0.55f + noiseSignalL + subBass) * env;
            float combinedR = (fmSignalR * 0.55f + noiseSignalR + subBass) * env;

            float lfoRate = isHeavy ? 3.0f : 5.0f;
            float lfo = (float)Math.Sin(2.0 * Math.PI * lfoRate * t);
            float delayTimeSecL = 0.003f + 0.002f * lfo;
            float delayTimeSecR = 0.004f - 0.002f * lfo;

            int delaySamplesL = Math.Max(1, Math.Min((int)(SampleRate * delayTimeSecL), maxDelaySamples - 1));
            int delaySamplesR = Math.Max(1, Math.Min((int)(SampleRate * delayTimeSecR), maxDelaySamples - 1));

            int readIndexL = (delayIndex - delaySamplesL + maxDelaySamples) % maxDelaySamples;
            int readIndexR = (delayIndex - delaySamplesR + maxDelaySamples) % maxDelaySamples;

            delayBufferL[delayIndex] = combinedL;
            delayBufferR[delayIndex] = combinedR;
            delayIndex = (delayIndex + 1) % maxDelaySamples;

            float flangedL = combinedL + delayBufferL[readIndexL] * 0.45f;
            float flangedR = combinedR + delayBufferR[readIndexR] * 0.45f;

            leftBuffer[i] = flangedL;
            rightBuffer[i] = flangedR;
        }

        float maxPeak = 0.0001f;
        for (int i = 0; i < totalSamples; i++)
        {
            float absL = Math.Abs(leftBuffer[i]);
            float absR = Math.Abs(rightBuffer[i]);
            if (absL > maxPeak) maxPeak = absL;
            if (absR > maxPeak) maxPeak = absR;
        }

        float targetPeak = 0.95f;
        float gain = targetPeak / maxPeak;
        for (int i = 0; i < totalSamples; i++)
        {
            leftBuffer[i] *= gain;
            rightBuffer[i] *= gain;
        }

        return EncodeWav(leftBuffer, rightBuffer, SampleRate);
    }

    private static byte[] EncodeWav(float[] left, float[] right, int sampleRate)
    {
        int samples = left.Length;
        int channels = 2;
        int bytesPerSample = 2;
        int dataChunkSize = samples * channels * bytesPerSample;

        using (MemoryStream ms = new MemoryStream(44 + dataChunkSize))
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(new char[] { 'R', 'I', 'F', 'F' });
            bw.Write(36 + dataChunkSize);
            bw.Write(new char[] { 'W', 'A', 'V', 'E' });

            bw.Write(new char[] { 'f', 'm', 't', ' ' });
            bw.Write(16);
            bw.Write((short)1);
            bw.Write((short)channels);
            bw.Write(sampleRate);
            bw.Write(sampleRate * channels * bytesPerSample);
            bw.Write((short)(channels * bytesPerSample));
            bw.Write((short)(bytesPerSample * 8));

            bw.Write(new char[] { 'd', 'a', 't', 'a' });
            bw.Write(dataChunkSize);

            for (int i = 0; i < samples; i++)
            {
                short valL = (short)Math.Max(-32768, Math.Min(32767, (int)(left[i] * 32767f)));
                short valR = (short)Math.Max(-32768, Math.Min(32767, (int)(right[i] * 32767f)));
                bw.Write(valL);
                bw.Write(valR);
            }

            return ms.ToArray();
        }
    }
}
"@

Add-Type -TypeDefinition $code -Language CSharp
$targetDir = Resolve-Path "../resources/SE"
[JuwanGenerator]::GenerateSEs($targetDir)

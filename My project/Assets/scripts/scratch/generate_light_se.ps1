$code = @"
using System;
using System.IO;

public static class LightShotGenerator
{
    private const int SampleRate = 44100;

    public static void Generate(string outputDir)
    {
        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
        string path = Path.Combine(outputDir, "SE_EnemyShot_Mob_Light.wav");
        byte[] wav = SynthesizeLightSE();
        File.WriteAllBytes(path, wav);
        Console.WriteLine("Generated Light Shot: " + path);
    }

    private static byte[] SynthesizeLightSE()
    {
        float duration = 0.20f;
        int totalSamples = (int)(SampleRate * duration);

        float[] leftBuffer = new float[totalSamples];
        float[] rightBuffer = new float[totalSamples];

        float startFreq = 1080f;
        float endFreq = 360f;
        float modRatio = 2.0f;
        float modIndexStart = 2.2f;

        float noiseCutoffStart = 1800f;
        float noiseCutoffEnd = 400f;
        float noiseResonance = 2.2f;

        Random rand = new Random(777);

        double carrierPhaseL = 0.0;
        double carrierPhaseR = 0.0;
        double modPhase = 0.0;

        float bpfLowL = 0f, bpfBandL = 0f;
        float bpfLowR = 0f, bpfBandR = 0f;

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / SampleRate;
            float progress = t / duration;

            // アタック極小、素早い減衰（タイト）
            float env;
            float attackTime = 0.002f;
            if (t < attackTime)
            {
                env = t / attackTime;
            }
            else
            {
                env = (float)Math.Exp(-(t - attackTime) * 16.0); // 急速減衰
            }

            // ピッチドロップ
            float pitchProgress = (float)Math.Pow(progress, 0.4);
            float currentFreq = startFreq + (endFreq - startFreq) * pitchProgress;
            float currentModFreq = currentFreq * modRatio;
            float currentModIndex = modIndexStart * (1f - progress);

            modPhase += 2.0 * Math.PI * currentModFreq / SampleRate;
            float modSignal = (float)Math.Sin(modPhase) * currentModIndex;

            carrierPhaseL += 2.0 * Math.PI * currentFreq / SampleRate;
            carrierPhaseR += 2.0 * Math.PI * (currentFreq * 0.995) / SampleRate;

            float toneL = (float)Math.Sin(carrierPhaseL + modSignal);
            float toneR = (float)Math.Sin(carrierPhaseR + modSignal);

            // 小さなエアパルス（シュッ）
            float whiteL = (float)(rand.NextDouble() * 2.0 - 1.0);
            float whiteR = (float)(rand.NextDouble() * 2.0 - 1.0);

            float cutoff = noiseCutoffStart + (noiseCutoffEnd - noiseCutoffStart) * progress;
            float f = 2f * (float)Math.Sin(Math.PI * cutoff / SampleRate);
            float q = 1f / noiseResonance;

            bpfLowL += f * bpfBandL;
            float bpfHighL = whiteL - bpfLowL - q * bpfBandL;
            bpfBandL += f * bpfHighL;

            bpfLowR += f * bpfBandR;
            float bpfHighR = whiteR - bpfLowR - q * bpfBandR;
            bpfBandR += f * bpfHighR;

            float noiseL = bpfBandL * 0.7f;
            float noiseR = bpfBandR * 0.7f;

            // 軽いアタッククリック（「プチッ」「シュッ」）
            float click = 0f;
            if (t < 0.003f)
            {
                click = (1f - t / 0.003f) * 0.4f;
            }

            // 合成（重低音なし・軽快・小粒）
            float outL = (toneL * 0.5f + noiseL + click) * env;
            float outR = (toneR * 0.5f + noiseR + click) * env;

            leftBuffer[i] = outL;
            rightBuffer[i] = outR;
        }

        // ピークノーマライズ（-2dBで控えめな音量）
        float maxPeak = 0.0001f;
        for (int i = 0; i < totalSamples; i++)
        {
            float absL = Math.Abs(leftBuffer[i]);
            float absR = Math.Abs(rightBuffer[i]);
            if (absL > maxPeak) maxPeak = absL;
            if (absR > maxPeak) maxPeak = absR;
        }

        float targetPeak = 0.82f;
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
[LightShotGenerator]::Generate($targetDir)

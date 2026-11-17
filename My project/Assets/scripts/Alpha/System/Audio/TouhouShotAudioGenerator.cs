using System;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Alpha.Audio
{
    /// <summary>
    /// シックで重厚な弾幕発射SE（ザコ用通常版 & ボス用強化版）を
    /// 数式・波形合成（ピンクノイズ・レゾナンス・エアプレッシャー・ステレオアンビエンス）で生成してWAV出力するジェネレーター
    /// </summary>
    [InitializeOnLoad]
    public static class TouhouShotAudioGenerator
    {
        private const int SampleRate = 44100;

        static TouhouShotAudioGenerator()
        {
            EditorApplication.delayCall += EnsureSEFilesExist;
        }

        private static void EnsureSEFilesExist()
        {
            string normalPath = Path.Combine(Application.dataPath, "resources", "SE", "SE_EnemyShot_Touhou_Juwan.wav");
            string heavyPath = Path.Combine(Application.dataPath, "resources", "SE", "SE_EnemyShot_Touhou_Juwan_Heavy.wav");
            if (!File.Exists(normalPath) || !File.Exists(heavyPath))
            {
                GenerateSEs();
            }
        }

        [MenuItem("Tools/Audio/Generate Touhou Shot SEs", priority = 100)]
        public static void GenerateAllSEsMenu()
        {
            GenerateSEs();
        }

        public static void GenerateSEs()
        {
            string outputDir = Path.Combine(Application.dataPath, "resources", "SE");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // 1. 通常版（ザコ敵用: シックで落ち着いた風圧・プラズマ弾幕発射音）
            string normalPath = Path.Combine(outputDir, "SE_EnemyShot_Touhou_Juwan.wav");
            byte[] normalWav = SynthesizeChicJuwanSE(isHeavy: false);
            File.WriteAllBytes(normalPath, normalWav);
            Debug.Log($"[TouhouAudioGenerator] Generated Chic Normal Shot SE: {normalPath}");

            // 2. 強化版（ボス・強攻撃用: 重厚サブベース & 深い空気減圧音「ズジュワァァーーン！」）
            string heavyPath = Path.Combine(outputDir, "SE_EnemyShot_Touhou_Juwan_Heavy.wav");
            byte[] heavyWav = SynthesizeChicJuwanSE(isHeavy: true);
            File.WriteAllBytes(heavyPath, heavyWav);
            Debug.Log($"[TouhouAudioGenerator] Generated Chic Heavy Shot SE: {heavyPath}");

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// シックで重厚な「じゅわーん！」SEの波形合成処理
        /// </summary>
        private static byte[] SynthesizeChicJuwanSE(bool isHeavy)
        {
            float duration = isHeavy ? 0.75f : 0.42f;
            int totalSamples = (int)(SampleRate * duration);

            float[] leftBuffer = new float[totalSamples];
            float[] rightBuffer = new float[totalSamples];

            // シックな中低域ピッチパラメータ
            float startFreq = isHeavy ? 540f : 960f;
            float endFreq = isHeavy ? 110f : 210f;
            float modRatio = isHeavy ? 1.5f : 2.0f;
            float modIndexStart = isHeavy ? 4.5f : 2.8f;

            // フィルターパラメータ（高周波を削り、空気の塊・風圧感を強調）
            float noiseCutoffStart = isHeavy ? 1800f : 1600f;
            float noiseCutoffEnd = isHeavy ? 160f : 340f;
            float noiseResonance = isHeavy ? 3.5f : 2.6f;

            // ステレオディレイバッファ（ワイドな空間感）
            int maxDelaySamples = (int)(SampleRate * 0.03f);
            float[] delayBufferL = new float[maxDelaySamples];
            float[] delayBufferR = new float[maxDelaySamples];
            int delayIndex = 0;

            float bpfLowL = 0f, bpfBandL = 0f;
            float bpfLowR = 0f, bpfBandR = 0f;

            // ピンクノイズ生成用フィルター変数
            float b0_L = 0f, b1_L = 0f, b2_L = 0f;
            float b0_R = 0f, b1_R = 0f, b2_R = 0f;

            System.Random rand = new System.Random(4242);

            double carrierPhaseL = 0.0;
            double carrierPhaseR = 0.0;
            double modPhase = 0.0;
            double subBasePhase = 0.0;

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;

                // 1. エンベロープ
                float env;
                float attackTime = 0.003f;
                if (t < attackTime)
                {
                    env = t / attackTime;
                }
                else
                {
                    float decayFactor = isHeavy ? 5.2f : 7.5f;
                    env = Mathf.Exp(-(t - attackTime) * decayFactor);
                }

                // 2. ピッチエンベロープ
                float pitchProgress = Mathf.Pow(progress, 0.45f);
                float currentFreq = Mathf.Lerp(startFreq, endFreq, pitchProgress);
                float currentModFreq = currentFreq * modRatio;
                float currentModIndex = modIndexStart * (1f - Mathf.Pow(progress, 0.6f));

                modPhase += 2.0 * Math.PI * currentModFreq / SampleRate;
                float modSignal = (float)Math.Sin(modPhase) * currentModIndex;

                float detune = isHeavy ? 2.0f : 1.0f;
                carrierPhaseL += 2.0 * Math.PI * (currentFreq + detune) / SampleRate;
                carrierPhaseR += 2.0 * Math.PI * (currentFreq - detune) / SampleRate;

                float rawCarrierL = (float)Math.Sin(carrierPhaseL + modSignal);
                float rawCarrierR = (float)Math.Sin(carrierPhaseR + modSignal);
                float toneSignalL = (float)Math.Tanh(rawCarrierL * 1.2);
                float toneSignalR = (float)Math.Tanh(rawCarrierR * 1.2);

                // 3. ピンクノイズ生成
                float wL = (float)(rand.NextDouble() * 2.0 - 1.0);
                float wR = (float)(rand.NextDouble() * 2.0 - 1.0);

                b0_L = 0.99765f * b0_L + wL * 0.0990460f;
                b1_L = 0.96300f * b1_L + wL * 0.2965164f;
                b2_L = 0.57000f * b2_L + wL * 1.0526913f;
                float pinkL = (b0_L + b1_L + b2_L + wL * 0.1848f) * 0.2f;

                b0_R = 0.99765f * b0_R + wR * 0.0990460f;
                b1_R = 0.96300f * b1_R + wR * 0.2965164f;
                b2_R = 0.57000f * b2_R + wR * 1.0526913f;
                float pinkR = (b0_R + b1_R + b2_R + wR * 0.1848f) * 0.2f;

                // フィルター周波数スイープ
                float cutoff = Mathf.Lerp(noiseCutoffStart, noiseCutoffEnd, Mathf.Pow(progress, 0.4f));
                float f = 2f * Mathf.Sin(Mathf.PI * cutoff / SampleRate);
                float q = 1f / noiseResonance;

                bpfLowL += f * bpfBandL;
                float bpfHighL = pinkL - bpfLowL - q * bpfBandL;
                bpfBandL += f * bpfHighL;

                bpfLowR += f * bpfBandR;
                float bpfHighR = pinkR - bpfLowR - q * bpfBandR;
                bpfBandR += f * bpfHighR;

                float noiseSignalL = bpfBandL * (isHeavy ? 0.95f : 0.8f);
                float noiseSignalR = bpfBandR * (isHeavy ? 0.95f : 0.8f);

                // 4. アタッククリック
                float click = 0f;
                if (t < 0.005f)
                {
                    click = (1f - t / 0.005f) * (isHeavy ? 0.6f : 0.4f);
                }

                // 5. サブベース（強化版）
                float subBass = 0f;
                if (isHeavy)
                {
                    float subFreq = Mathf.Lerp(65f, 26f, progress);
                    subBasePhase += 2.0 * Math.PI * subFreq / SampleRate;
                    float subEnv = Mathf.Exp(-t * 4.8f);
                    subBass = (float)Math.Sin(subBasePhase) * subEnv * 0.85f;
                }

                // 6. ミックス
                float combinedL = (toneSignalL * 0.45f + noiseSignalL + subBass + click) * env;
                float combinedR = (toneSignalR * 0.45f + noiseSignalR + subBass + click) * env;

                // 7. ステレオアンビエンス
                int delaySamplesL = (int)(SampleRate * 0.007f);
                int delaySamplesR = (int)(SampleRate * 0.013f);

                int readIndexL = (delayIndex - delaySamplesL + maxDelaySamples) % maxDelaySamples;
                int readIndexR = (delayIndex - delaySamplesR + maxDelaySamples) % maxDelaySamples;

                delayBufferL[delayIndex] = combinedL;
                delayBufferR[delayIndex] = combinedR;
                delayIndex = (delayIndex + 1) % maxDelaySamples;

                float outL = combinedL + delayBufferL[readIndexL] * 0.35f;
                float outR = combinedR + delayBufferR[readIndexR] * 0.35f;

                leftBuffer[i] = outL;
                rightBuffer[i] = outR;
            }

            // ピークノーマライズ
            float maxPeak = 0.0001f;
            for (int i = 0; i < totalSamples; i++)
            {
                float absL = Math.Abs(leftBuffer[i]);
                float absR = Math.Abs(rightBuffer[i]);
                if (absL > maxPeak) maxPeak = absL;
                if (absR > maxPeak) maxPeak = absR;
            }

            float targetPeak = 0.92f;
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
                    short valL = (short)Mathf.Clamp(left[i] * 32767f, -32768f, 32767f);
                    short valR = (short)Mathf.Clamp(right[i] * 32767f, -32768f, 32767f);
                    bw.Write(valL);
                    bw.Write(valR);
                }

                return ms.ToArray();
            }
        }
    }
}
#endif

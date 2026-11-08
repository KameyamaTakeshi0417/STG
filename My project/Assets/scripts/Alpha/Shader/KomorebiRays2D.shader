Shader "Custom/KomorebiRays2D"
{
    Properties
    {
        [Header(Light Color and Intensity)]
        _RayColor ("Ray Color", Color) = (1.0, 0.95, 0.85, 1.0)
        _Intensity ("Light Intensity", Range(0, 5)) = 1.5
        _Alpha ("Overall Opacity", Range(0, 1)) = 0.5

        [Header(Straight Ray Structure)]
        _RayAngle ("Ray Angle (Degrees)", Range(-75, 75)) = 25.0
        _RayCount ("Ray Frequency / Density", Range(1, 30)) = 8.0
        _RaySharpness ("Ray Sharpness (Beam Tightness)", Range(1, 15)) = 4.0
        _RayContrast ("Ray Contrast", Range(0.5, 3)) = 1.2

        [Header(Light Beam Attenuation)]
        _RayLength ("Beam Reach / Length", Range(0.2, 2.0)) = 1.2
        _RayFalloffSoftness ("Falloff Softness", Range(0.1, 2.0)) = 0.8
        _TopFade ("Top Origin Fade", Range(0, 0.5)) = 0.05

        [Header(Subtle Canopy Wind Flicker)]
        _FlickerSpeed ("Wind / Flicker Speed", Range(0, 5)) = 0.6
        _FlickerStrength ("Wind Shadow Strength", Range(0, 1)) = 0.35
        _FlickerScale ("Leaf Shadow Scale", Range(0.5, 10)) = 3.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent+500"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }

        LOD 100
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha One // 加算ブレンドで綺麗な発光

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _RayColor;
            float _Intensity;
            float _Alpha;

            float _RayAngle;
            float _RayCount;
            float _RaySharpness;
            float _RayContrast;

            float _RayLength;
            float _RayFalloffSoftness;
            float _TopFade;

            float _FlickerSpeed;
            float _FlickerStrength;
            float _FlickerScale;

            // 葉のざわめき用シンプルノイズ
            float hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float noise1D(float x)
            {
                float i = floor(x);
                float f = frac(x);
                float u = f * f * (3.0 - 2.0 * f);
                return lerp(hash11(i), hash11(i + 1.0), u);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // 1. 直線的な傾き（真っ直ぐな平行光線）
                float rad = radians(_RayAngle);
                float tanA = tan(rad);
                // 上から下へ向かって真っ直ぐ射すUV軸
                float beamCoord = uv.x + (1.0 - uv.y) * tanA;

                // 2. 異なる周波数の波を重ねて「太い光」と「細い光」が混ざる直線的なビームを生成
                float b1 = sin(beamCoord * _RayCount * 6.28318);
                float b2 = sin(beamCoord * (_RayCount * 1.73) * 6.28318 + 1.5);
                float b3 = sin(beamCoord * (_RayCount * 0.47) * 6.28318 + 3.14);

                // 0〜1に正規化
                float rawBeams = (b1 * 0.5 + b2 * 0.3 + b3 * 0.2) * 0.5 + 0.5;

                // シャープネスとコントラストで「くっきりとした光条」にする
                rawBeams = saturate((rawBeams - 0.2) / 0.8);
                float straightRays = pow(rawBeams, _RaySharpness) * _RayContrast;

                // 3. 上空の木漏れ日（葉が揺れて光がチラチラと明滅する効果）
                // 筋自体は絶対に歪ませず、光の明るさだけをアニメーションさせる
                float time = _Time.y * _FlickerSpeed;
                float leafFlicker = noise1D(beamCoord * _FlickerScale + time) * 0.6
                                  + noise1D(beamCoord * (_FlickerScale * 2.3) - time * 1.4) * 0.4;

                // 影のチラつきをブレンド
                float shadow = lerp(1.0, leafFlicker, _FlickerStrength);
                straightRays *= shadow;

                // 4. 光源（上）からの減衰（ビームが下に向かってすーっと消えていく）
                float distanceFade = saturate((1.0 - uv.y) / max(0.001, _RayLength));
                float beamFalloff = pow(saturate(1.0 - distanceFade), _RayFalloffSoftness);

                // 画面最上部がパキッと切れないためのフェード
                float topFade = smoothstep(1.0, 1.0 - _TopFade, uv.y);

                float finalMask = straightRays * beamFalloff * topFade;

                // 5. 最終カラー
                float finalAlpha = saturate(finalMask * _Alpha);
                fixed3 finalRGB = _RayColor.rgb * _Intensity * finalAlpha;

                return fixed4(finalRGB, finalAlpha);
            }
            ENDCG
        }
    }
}

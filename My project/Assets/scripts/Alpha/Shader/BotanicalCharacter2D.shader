Shader "Custom/BotanicalCharacter2D"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

        [Header(Top Down Lighting (Volumetric Gradient))]
        _TopColor ("Top Light Tint", Color) = (1.05, 1.04, 1.02, 1.0)
        _BottomColor ("Bottom Shade Tint", Color) = (0.92, 0.94, 0.96, 1.0)
        _LightHeight ("Light Gradient Center", Range(0, 1)) = 0.45
        _LightSmooth ("Light Gradient Smoothness", Range(0.1, 1)) = 0.65

        [Header(Edge Rim Light (Silhouette Separation))]
        _RimColor ("Rim Light Color", Color) = (1.0, 0.96, 0.88, 1.0)
        _RimIntensity ("Rim Intensity", Range(0, 5)) = 0.45
        _RimWidth ("Rim Width (Pixels)", Range(0.5, 4.0)) = 1.2

        [Header(Komorebi Canopy Shimmer)]
        _KomorebiColor ("Komorebi Tint", Color) = (1.0, 0.97, 0.90, 1.0)
        _KomorebiStrength ("Komorebi Strength", Range(0, 1)) = 0.05
        _KomorebiSpeed ("Komorebi Shimmer Speed", Range(0, 5)) = 1.0
        _KomorebiScale ("Komorebi Noise Scale", Range(0.5, 10)) = 2.0

        [Header(Hit Flash)]
        _FlashColor ("Hit Flash Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _FlashAmount ("Hit Flash Amount", Range(0, 1)) = 0.0

        [Header(Petal Dissolve)]
        _DissolveAmount ("Dissolve Progress", Range(0, 1)) = 0.0
        _DissolveScale ("Dissolve Pattern Scale", Range(5, 50)) = 22.0
        _DissolveEdgeColor ("Dissolve Edge Glow", Color) = (0.95, 0.78, 0.35, 1.0)
        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0.01, 0.2)) = 0.08
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Sprite base properties
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            sampler2D _AlphaTex;
            float _EnableExternalAlpha;
            fixed4 _Color;
            fixed4 _RendererColor;

            // Custom Lighting properties
            fixed4 _TopColor;
            fixed4 _BottomColor;
            float _LightHeight;
            float _LightSmooth;

            // Rim Light properties
            fixed4 _RimColor;
            float _RimIntensity;
            float _RimWidth;

            // Komorebi shimmer properties
            fixed4 _KomorebiColor;
            float _KomorebiStrength;
            float _KomorebiSpeed;
            float _KomorebiScale;

            // Hit flash
            fixed4 _FlashColor;
            float _FlashAmount;

            // Dissolve
            float _DissolveAmount;
            float _DissolveScale;
            fixed4 _DissolveEdgeColor;
            float _DissolveEdgeWidth;

            // Procedural noise function for petal-like organic disintegration
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            fixed4 SampleSpriteTexture(float2 uv)
            {
                fixed4 color = tex2D(_MainTex, uv);

                #if ETC1_EXTERNAL_ALPHA
                if (_EnableExternalAlpha)
                {
                    color.a = tex2D(_AlphaTex, uv).r;
                }
                #endif

                return color;
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex);
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color * _RendererColor;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;

                // アルファがほぼ0なら描画破棄
                if (c.a <= 0.01) discard;

                // 1. 花弁ディゾルブ (Petal Dissolve)
                if (_DissolveAmount > 0.001)
                {
                    float n = noise(IN.texcoord * _DissolveScale);
                    if (n < _DissolveAmount)
                    {
                        discard;
                    }

                    // 境界の黄金・花弁グロー発光
                    float edgeDist = n - _DissolveAmount;
                    if (edgeDist < _DissolveEdgeWidth)
                    {
                        float edgeFactor = 1.0 - (edgeDist / _DissolveEdgeWidth);
                        c.rgb += _DissolveEdgeColor.rgb * edgeFactor * 2.5;
                    }
                }

                // 2. トップダウン立体ライティング (Volumetric Gradient)
                // 上から差し込む柔らかい光と足元への落ち着いた環境陰影
                float halfSmooth = max(0.01, _LightSmooth * 0.5);
                float gradT = smoothstep(_LightHeight - halfSmooth, _LightHeight + halfSmooth, IN.texcoord.y);
                fixed3 ambientLight = lerp(_BottomColor.rgb, _TopColor.rgb, gradT);
                c.rgb *= ambientLight;

                // 3. 木漏れ日の揺らぎ (Komorebi Canopy Shimmer)
                if (_KomorebiStrength > 0.001)
                {
                    float kWave = sin(IN.worldPos.y * _KomorebiScale + IN.worldPos.x * (_KomorebiScale * 0.7) + _Time.y * _KomorebiSpeed) * 0.5 + 0.5;
                    c.rgb += _KomorebiColor.rgb * kWave * _KomorebiStrength;
                }

                // 4. エッジ・リムライト (Silhouette Separation Rim Light)
                if (_RimIntensity > 0.001)
                {
                    float2 offset = _MainTex_TexelSize.xy * _RimWidth;
                    float aU = SampleSpriteTexture(IN.texcoord + float2(0, offset.y)).a;
                    float aD = SampleSpriteTexture(IN.texcoord - float2(0, offset.y)).a;
                    float aR = SampleSpriteTexture(IN.texcoord + float2(offset.x, 0)).a;
                    float aL = SampleSpriteTexture(IN.texcoord - float2(offset.x, 0)).a;

                    float rimAlpha = 1.0 - saturate(aU * aD * aR * aL);
                    fixed3 rimLight = _RimColor.rgb * rimAlpha * _RimIntensity;
                    c.rgb += rimLight;
                }

                // 5. ヒットフラッシュ (Hit Flash)
                if (_FlashAmount > 0.001)
                {
                    c.rgb = lerp(c.rgb, _FlashColor.rgb, _FlashAmount);
                }

                // Premultiply Alpha (Blend One OneMinusSrcAlpha)
                c.rgb *= c.a;

                return c;
            }
            ENDCG
        }
    }
}

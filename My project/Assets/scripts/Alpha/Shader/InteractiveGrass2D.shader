Shader "Alpha/InteractiveGrass2D"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Wind Settings)]
        _WindSpeed ("Wind Speed", Float) = 2.0
        _WindStrength ("Wind Strength", Float) = 0.1
        
        [Header(Interaction Settings)]
        _InteractRadius ("Interact Radius", Float) = 1.5
        _InteractStrength ("Interact Strength", Float) = 0.5

        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
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
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Properties
            fixed4 _Color;
            float _WindSpeed;
            float _WindStrength;
            float _InteractRadius;
            float _InteractStrength;
            
            // Sprite specific
            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float _EnableExternalAlpha;
            fixed4 _RendererColor;
            
            // Global Variable set by script
            float3 _PlayerPos;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                // --- START CUSTOM VERTEX LOGIC ---
                // We calculate the effect in World Space, but apply it to Object Space vertex.
                float4 worldPos = mul(unity_ObjectToWorld, IN.vertex);
                
                // 1. Wind calculation based on world X and time
                float wind = sin(_Time.y * _WindSpeed + worldPos.x * 0.5) * _WindStrength;

                // 2. Player interaction calculation
                float interact = 0.0;
                float dist = distance(worldPos.xy, _PlayerPos.xy);
                
                if (dist < _InteractRadius)
                {
                    // Direction from player to grass
                    float dir = sign(worldPos.x - _PlayerPos.x); 
                    if (dir == 0) dir = 1; // Prevent 0
                    
                    // Closer = stronger push. Falloff linearly.
                    interact = (1.0 - (dist / _InteractRadius)) * _InteractStrength * dir;
                }

                // 3. Apply bending only to the top vertices
                // Assume the pivot is at the bottom (IN.vertex.y >= 0).
                float bendFactor = max(0, IN.vertex.y);
                
                // Add the horizontal bend (X axis in object space)
                IN.vertex.x += (wind + interact) * bendFactor;
                // --- END CUSTOM VERTEX LOGIC ---

                // Standard Sprite Transform
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color * _RendererColor;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                if (_EnableExternalAlpha) {
                    c.a = tex2D(_AlphaTex, IN.texcoord).r * IN.color.a;
                }
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}

Shader "Custom/UnlitTint"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        // Background キュー(1000)で描画 → スプライト/UI(Transparent=3000)より必ず先に描かれ、最背面になる
        Tags { "RenderType"="Opaque" "Queue"="Background" "IgnoreProjector"="True" }
        LOD 100
        Cull Off
        ZWrite Off   // 深度を書かない → 手前のオブジェクトを隠さない
        ZTest Always // 自身は常に描画される
        Blend SrcAlpha OneMinusSrcAlpha

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Tiling と Offset を適用（リピート・スクロール用）
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // テクスチャ色に Tint Color を乗算（暗くする）
                return tex2D(_MainTex, i.uv) * _Color;
            }
            ENDCG
        }
    }
}

// Shader de outline exterior para sprites 2D.
// Renderiza el color de outline SOLO en píxeles transparentes adyacentes al sprite.
// Los píxeles interiores (cualquier alpha) se vuelven transparentes → el sprite principal los maneja.
Shader "Custom/SpriteSilhouette"
{
    Properties
    {
        _MainTex     ("Sprite Texture", 2D)    = "white" {}
        _Color       ("Outline Color",  Color) = (1,1,1,1)
        _OutlineSize ("Outline Pixels", Float) = 1.5
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "RenderType"      = "Transparent"
            "IgnoreProjector" = "True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        // ── URP pass ────────────────────────────────────────────────────────
        Pass
        {
            Name "SpriteOutlineURP"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _Color;
                float  _OutlineSize;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize; // (1/w, 1/h, w, h) — auto-set by Unity

            struct Attributes { float4 posOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 posCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posCS = TransformObjectToHClip(IN.posOS.xyz);
                OUT.uv    = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half a = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).a;

                // Pixel interior al sprite → transparente (el sprite principal lo cubre)
                if (a > 0.01)
                    return half4(0, 0, 0, 0);

                // Pixel transparente: ¿algún vecino tiene alpha?
                float2 t = _MainTex_TexelSize.xy * _OutlineSize;
                half mx = 0;
                mx = max(mx, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( t.x,    0)).a);
                mx = max(mx, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-t.x,    0)).a);
                mx = max(mx, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(   0,  t.y)).a);
                mx = max(mx, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(   0, -t.y)).a);
                mx = max(mx, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( t.x,  t.y)).a);
                mx = max(mx, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-t.x,  t.y)).a);
                mx = max(mx, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( t.x, -t.y)).a);
                mx = max(mx, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-t.x, -t.y)).a);

                return mx > 0.01 ? _Color : half4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    }

    // ── Fallback Built-in ────────────────────────────────────────────────────
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4    _MainTex_ST;
            float4    _MainTex_TexelSize;
            fixed4    _Color;
            float     _OutlineSize;

            struct appdata { float4 v : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float4 v : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata i)
            {
                v2f o;
                o.v  = UnityObjectToClipPos(i.v);
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed a = tex2D(_MainTex, i.uv).a;
                if (a > 0.01) return fixed4(0, 0, 0, 0);

                float2 t = _MainTex_TexelSize.xy * _OutlineSize;
                fixed mx = 0;
                mx = max(mx, tex2D(_MainTex, i.uv + float2( t.x,    0)).a);
                mx = max(mx, tex2D(_MainTex, i.uv + float2(-t.x,    0)).a);
                mx = max(mx, tex2D(_MainTex, i.uv + float2(    0,  t.y)).a);
                mx = max(mx, tex2D(_MainTex, i.uv + float2(    0, -t.y)).a);
                mx = max(mx, tex2D(_MainTex, i.uv + float2( t.x,  t.y)).a);
                mx = max(mx, tex2D(_MainTex, i.uv + float2(-t.x,  t.y)).a);
                mx = max(mx, tex2D(_MainTex, i.uv + float2( t.x, -t.y)).a);
                mx = max(mx, tex2D(_MainTex, i.uv + float2(-t.x, -t.y)).a);

                return mx > 0.01 ? _Color : fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}

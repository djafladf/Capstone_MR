Shader "Hidden/OutLiner"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutColor ("LineCOlor",Color) = (1,1,1,1)
        _Depth ("LineDepth",float) = 1
        _Blend("ColorBlend",Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
    
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            half4 _OutColor;
            half4 _Blend;
            float4 _MainTex_TexelSize;
            float _Depth;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 adj = i.uv;
                float alphaSum = 1;
                float2 ts = _MainTex_TexelSize.xy * _Depth;
                half4 CurPixel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                if (CurPixel.a == 0) discard;

                adj.y = i.uv.y + ts.y;                 alphaSum *= SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, adj).a * step(adj.y,1);
                adj.x = i.uv.x - ts.x; adj.y = i.uv.y; alphaSum *= SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, adj).a * step(0,adj.x);
                adj.x = i.uv.x + ts.x;                 alphaSum *= SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, adj).a * step(adj.x,1);
                adj.x = i.uv.x; adj.y = i.uv.y - ts.y;  alphaSum *= SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, adj).a * step(0, adj.y);
                
                alphaSum = step(alphaSum, 0);

                
                half4 final = lerp(CurPixel * _Blend, _OutColor, alphaSum);

                return final;
            }
            ENDHLSL
        }
    }
}

Shader "Hidden/ReverseShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Reverse", Color) = (1,1,1,1)
        _Threshold ("Thres", Range(0,1)) = 0
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
            half4 _Color;
            float _Threshold;

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
                float2 Changeduv = (i.uv - 0.5) * 1.8 + 0.5;
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, Changeduv);
                col *= step(Changeduv.x,1) * step(0,Changeduv.x) * step(Changeduv.y,1) * step(0,Changeduv.y);
                float dist = distance(float2(0.5, 0.5), i.uv);
                _Color.a = (1.0 - step(_Threshold, col.a)) * smoothstep(0.5, 0.45, dist);
                return _Color;
            }
            ENDHLSL
        }
    }
}

Shader "Hidden/GradColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorT ("Top", Color) = (1,1,1,1)
        _ColorB ("Bottom",Color) = (1,1,1,1)
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
            half4 _ColorT,_ColorB;
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
                half4 Color = i.uv.y * _ColorT + (1-i.uv.y) * _ColorB;
                return Color;
            }
            ENDHLSL
        }
    }
}

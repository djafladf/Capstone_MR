Shader "Hidden/DonutGraph"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorNum ("ColorNum",Int) = 3
        _Depth ("Depth",Range(0.1,0.3)) = 0.1
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
            half4 _Colors[10];
            float _Amounts[10];
            int _ColorNum;
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
                float dist = distance(i.uv, float2(0.5,0.5));
                if (step(dist, 0.5) * step(_Depth, dist) == 0) discard;

                float2 center = float2(0.5, 0.5);
                float2 dir = normalize(i.uv - center);

                
                float angleRad = atan2(dir.x, dir.y);
                float angleDeg = degrees(angleRad) + step(angleRad, 0) * 360.0;
                angleDeg /= 360.0;
                half4 color = half4(1,1,1,1);
                for (int i = 0; i < _ColorNum; i++)
                {
                    if (angleDeg <= _Amounts[i])
                    {
                        color = _Colors[i];
                        break;
                     }
                }
                
                return color;
            }
            ENDHLSL
        }
    }
}

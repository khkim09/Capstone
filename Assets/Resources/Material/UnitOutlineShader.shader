Shader "Custom/2DOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,1,0,1) // 초록색 기본값
        _OutlineThickness ("Outline Thickness", Float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // 투명도 처리

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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _OutlineColor;
            float _OutlineThickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 centerUV = i.uv;
                fixed4 centerColor = tex2D(_MainTex, centerUV);

                // 외곽선 검출을 위한 UV 오프셋 (8방향 샘플링)
                float2 offsets[8] = {
                    float2(-_OutlineThickness, 0),
                    float2(_OutlineThickness, 0),
                    float2(0, -_OutlineThickness),
                    float2(0, _OutlineThickness),
                    float2(-_OutlineThickness, -_OutlineThickness),
                    float2(-_OutlineThickness, _OutlineThickness),
                    float2(_OutlineThickness, -_OutlineThickness),
                    float2(_OutlineThickness, _OutlineThickness)
                };

                // 주변 픽셀들을 샘플링하여 외곽선 검출
                for (int j = 0; j < 8; ++j)
                {
                    float2 sampleUV = centerUV + offsets[j];
                    fixed4 sampleColor = tex2D(_MainTex, sampleUV);

                    // 현재 픽셀이 투명하지 않고 주변 픽셀이 투명하다면 외곽선으로 처리
                    if (centerColor.a > 0.0 && sampleColor.a <= 0.0)
                    {
                        return _OutlineColor;
                    }
                    // 현재 픽셀이 투명하고 주변 픽셀이 불투명하다면 외곽선으로 처리 (선택 사항)
                    // else if (centerColor.a <= 0.0 && sampleColor.a > 0.0)
                    // {
                    //     return _OutlineColor;
                    // }
                }

                // 외곽선이 아닌 경우 원래 텍스처 색상 반환
                return centerColor;
            }
            ENDCG
        }
    }
}
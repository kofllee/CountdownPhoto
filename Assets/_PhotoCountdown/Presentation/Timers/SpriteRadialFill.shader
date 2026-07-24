Shader "Photo Countdown/Sprite Radial Fill"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 1
        _StartAngle ("Start Angle", Range(0, 360)) = 0
        _Clockwise ("Clockwise", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _FillAmount;
                float _StartAngle;
                float _Clockwise;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                color *= input.color;

                float2 centeredUv = input.uv - 0.5;
                float angle = atan2(centeredUv.x, centeredUv.y);
                float angle01 = frac(angle / 6.28318530718 + 1.0);

                float start01 = _StartAngle / 360.0;
                float shifted = frac(angle01 - start01 + 1.0);

                if (_Clockwise < 0.5)
                    shifted = 1.0 - shifted;

                clip(_FillAmount - shifted);
                clip(color.a - 0.001);

                return color;
            }

            ENDHLSL
        }
    }
}
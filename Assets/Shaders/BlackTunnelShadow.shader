Shader "DebtPit/BlackTunnelShadow"
{
    Properties
    {
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 1)
        _FadeStart ("Tunnel Fade Start", Range(0, 1)) = 0.18
        _FadeEnd ("Tunnel Fade End", Range(0, 1)) = 0.92
        _MaxOpacity ("Maximum Opacity", Range(0, 1)) = 0.82
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Tunnel Shadow"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ShadowColor;
                float _FadeStart;
                float _FadeEnd;
                float _MaxOpacity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float2 positionXZOS : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionXZOS = input.positionOS.xz;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Hide the four vertical cube faces: they were the visible
                // rectangular black panels. Keep only the ceiling/floor plane.
                clip(abs(input.normalWS.y) - 0.9);

                // From below, this reads as a tunnel opening: transparent in
                // the middle and gradually darker toward the edge of the cube.
                float edgeDistance = max(abs(input.positionXZOS.x), abs(input.positionXZOS.y)) * 2.0;
                half alpha = smoothstep(_FadeStart, _FadeEnd, edgeDistance) * _MaxOpacity;
                return half4(_ShadowColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}

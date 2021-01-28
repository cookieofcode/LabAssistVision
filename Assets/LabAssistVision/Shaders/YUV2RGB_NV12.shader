// Adapted from https://github.com/microsoft/Windows-universal-samples/blob/1ad51db378ef34fb35078e3ddd4cf94cb717d6eb/Samples/HolographicFaceTracking/cpp/Content/Shaders/QuadPixelShaderNV12.hlsl
Shader "Unlit/YUV2RGB_NV12"
{
	Properties
	{
		luminanceChannel("Texture", 2D) = "black" {}
		chrominanceChannel("Texture", 2D) = "black" {}
	}
		SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 texCoord : TEXCOORD0;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.pos = UnityObjectToClipPos(v.vertex);
				o.texCoord = v.uv;
				return o;
			}
			struct PixelShaderInput
			{
				float4 pos         : SV_POSITION;
				float2 texCoord    : TEXCOORD0;
			};
			sampler2D luminanceChannel   : t0;
			sampler2D chrominanceChannel : t1;

			// from https://github.com/microsoft/Windows-universal-samples/blob/1ad51db378ef34fb35078e3ddd4cf94cb717d6eb/Samples/HolographicFaceTracking/cpp/Content/Shaders/QuadPixelShaderNV12.hlsl
			static const float3x3 YUVtoRGBCoeffMatrix =
			{
				1.164383f,  1.164383f, 1.164383f,
				0.000000f, -0.391762f, 2.017232f,
				1.596027f, -0.812968f, 0.000000f
			};

			// from https://github.com/microsoft/Windows-universal-samples/blob/1ad51db378ef34fb35078e3ddd4cf94cb717d6eb/Samples/HolographicFaceTracking/cpp/Content/Shaders/QuadPixelShaderNV12.hlsl
			float3 ConvertYUVtoRGB(float3 yuv)
			{
				// Derived from https://msdn.microsoft.com/en-us/library/windows/desktop/dd206750(v=vs.85).aspx
				// Section: Converting 8-bit YUV to RGB888

				// These values are calculated from (16 / 255) and (128 / 255)
				yuv -= float3(0.062745f, 0.501960f, 0.501960f);
				yuv = mul(yuv, YUVtoRGBCoeffMatrix);

				return saturate(yuv);
			}

			float4 frag(v2f input) : SV_TARGET
			{
				float y = tex2D(luminanceChannel, input.texCoord);
				float2 uv = tex2D(chrominanceChannel, input.texCoord);

				return float4(ConvertYUVtoRGB(float3(y, uv)), 1.f);
			}
			ENDCG
		}
	}
}
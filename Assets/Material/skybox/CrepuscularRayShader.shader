
Shader "Custom/CrepuscularRays" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_NumSamples("Number of Damples", Range(0, 1024)) = 128
		_Density("Density", Range(0, 1)) = 1.0
		_Weight("Weight", Range(0, 1)) = 1.0
		_Decay("Decay", Range(0, 1)) = 1.0
		_Exposure("Exposure", Range(0, 1)) = 1.0
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _CameraDepthTexture;
			float3 _LightPos;
			float _NumSamples;
			float _Density;
			float _Weight;
			float _Decay;
			float _Exposure;

			float4 frag(v2f_img i) : COLOR{
				// Calculate vector from pixel to light source in screen space.
				float4 light = float4(_LightPos.xyz,1);
				half2 deltaTexCoord = (i.uv - light.xy) * (light.z < 0 ? -1 : 1);
				// Divide by number of samples and scale by control factor.
				deltaTexCoord *= 1.0f / _NumSamples * _Density;
				// Store initial sample.
				half2 uv = i.uv;
				half3 color = tex2D(_MainTex, uv);
				half depth = Linear01Depth(tex2D(_CameraDepthTexture, uv).r);
				// Set up illumination decay factor.
				half illuminationDecay = 1.0f;
				// Evaluate summation from Equation 3 NUM_SAMPLES iterations.
				for (int i = 0; i < (light.z < 0 ? 0 : _NumSamples * light.z); i++)
				{
					// Step sample location along ray.
					uv -= deltaTexCoord;
					// Retrieve sample at new location.
					half3 sample = tex2D(_MainTex, uv);
					half depth2 = Linear01Depth(tex2D(_CameraDepthTexture, uv).r);
					// Apply sample attenuation scale/decay factors.
					sample *= illuminationDecay * (_Weight/ _NumSamples);
					// Accumulate combined color.
					color += sample;
					// Update exponential decay factor.
					illuminationDecay *= _Decay;
				}
				// Output final color with a further scale control factor.
				return float4(color * _Exposure, 1);
			}
		ENDCG
		}
	}
}
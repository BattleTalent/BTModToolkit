// Custom Inputs are X = Pan Offset, Y = UV Warp Strength, Z = Gravity
// Specular Alpha is used like a metalness control. High values are more like dielectrics, low are more like metals
// Subshader at the bottom is for Shader Model 2.0 and OpenGL ES 2.0 devices

Shader "Particles/FXVille Blood 2018"
{
	Properties
	{
		[Header (Color Controls)]
		[HDR] _BaseColor ("Base Color Mult", Color) = (1,1,1,1)
		_LightStr ("Lighting Strength", float) = 1.0
		_AlphaMin ("Alpha Clip Min", Range (-0.01, 1.01)) = 0.1
		_AlphaSoft ("Alpha Clip Softness", Range (0,1)) = 0.1
		_EdgeDarken ("Edge Darkening", float) = 1.0
		_ProcMask ("Procedural Mask Strength", float) = 1.0

		[Header (Mask Controls)]
		_MainTex ("Mask Texture", 2D) = "white" {}
		_MaskStr ("Mask Strength", float) = 0.7
		_Columns ("Flipbook Columns", Int) = 1
		_Rows ("Flipbook Rows", Int) = 1
		_ChannelMask ("Channel Mask", Vector) = (1,1,1,0)
		[Toggle] _FlipU("Flip U Randomly", float) = 0
		[Toggle] _FlipV("Flip V Randomly", float) = 0

		[Header (Noise Controls)]
		_NoiseTex ("Noise Texture", 2D) = "white" {}
		_NoiseAlphaStr ("Noise Strength", float) = 1.0
		_ChannelMask2 ("Channel Mask",Vector) = (1,1,1,0)
		_Randomize ("Randomize Noise", float) = 1.0

		[Header (UV Warp Controls)]
		_WarpTex ("Warp Texture", 2D) = "gray" {}
		_WarpStr ("Warp Strength", float) = 0.2

		[Header (Vertex Physics)]
		_FallOffset ("Gravity Offset", range(-1,0)) = -0.5
		_FallRandomness ("Gravity Randomness", float) = 0.25

		//specular stuff//
		[HDR] _SpecularColor ("Reflection Color Mult", Color) = (1,1,1,0.5)
		_ReflectionTex ("Reflection Texture", 2D) = "black" {}
		_ReflectionSat ("Reflection Saturation", float) = 0.5
		[NoScaleOffset] [Normal] _Normal ("Reflection Normalmap", 2D) = "bump" {}
		_FlattenNormal ("Flatten Reflection Normal", float) = 2.0 

	}
	Category {

		Tags 
		{
			"IgnoreProjector"="True"
			"Queue"="Transparent"
			"RenderType"="Transparent"
			"LightMode"="UniversalForward" 
		}

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		SubShader
		{

			LOD 100
			Pass 
			{
				Name "FORWARD"
				Tags 
				{
					"BW"="TrueProbes"
				}
				

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#ifndef UNITY_PASS_FORWARDBASE
				#define UNITY_PASS_FORWARDBASE
				#endif
				#include "UnityCG.cginc"
				#include "UnityShaderVariables.cginc"
				#include "HLSLSupport.cginc"
				#include "Lighting.cginc"
				#pragma multi_compile_fwdbase
				#pragma multi_compile SPECULAR_REFLECTION_ON SPECULAR_REFLECTION_OFF

				#pragma multi_compile_fog
				#pragma target 3.0

				/// Properties ///
				/// Color Controls ///
				half4 _BaseColor;
				half4 _SpecularColor;									
				half _LightStr;
				half _AlphaMin;
				half _AlphaSoft;
				half _EdgeDarken;
				half _ProcMask;

				/// Mask Controls ///
				sampler2D _MainTex; float4 _MainTex_ST;
				half _MaskStr;
				half _Columns;
				half _Rows;
				half4 _ChannelMask;
				half _FlipU;
				half _FlipV;

				sampler2D _ReflectionTex; float4 _ReflectionTex_ST; 	
				half _ReflectionSat;									

				/// Noise Controls ///
				sampler2D _NoiseTex; float4 _NoiseTex_ST;
				half _NoiseAlphaStr;
				half _NoiseColorStr;
				half4 _ChannelMask2;
				#ifdef SPECULAR_REFLECTION_ON
					sampler2D _Normal;										
					half _FlattenNormal;									
				#endif
				half _Randomize;

				/// UV Warp Controls ///
				sampler2D _WarpTex; float4 _WarpTex_ST;
				half _WarpStr;

				/// Vertex Physics ///
				half _FallOffset;
				half _FallRandomness;

				////// Incoming from Unity //////
				struct appdata
				{
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float4 texcoord0 : TEXCOORD0; // Z is Random, W is Lifetime
					float3 texcoord1 : TEXCOORD1; // X is Pan Offset, Y is UV Warp Strength, Z is Gravity


					float4 color : COLOR;
					#ifdef SPECULAR_REFLECTION_ON
						half4 tangent : TANGENT;							
					#endif
				};

				////// From Vertex Shader to Fragment Shader /////
				struct v2f
				{
					float4 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
					float4 color : Color;

					////// Stuff I constructed //////
					#ifdef SPECULAR_REFLECTION_ON
						float3 viewDir : TEXCOORD1;							
					#endif

					float4 vertLight : TEXCOORD3;
					float3 customData : TEXCOORD4; //XY is custom ((panDistanceOffset & warpStrength)), Z is stable random

					UNITY_FOG_COORDS(5)

					////// Normal Map Transform Stuff /////
					half3 normal : NORMAL;
					float3x3 tangentToWorld : TEXCOORD6;
				};


				////// VERTEX PROGRAM //////
				v2f vert (appdata v)
				{
					// pass a bunch of stuff straight through //
					v2f o = (v2f)0;

					float lifetime = v.texcoord0.w;
					lifetime = lifetime * lifetime + (_FallOffset + ((v.texcoord0.z-0.5) * _FallRandomness)) * lifetime;
					float4 fallPos = lifetime * float4(0, v.texcoord1.z,0,0);

					float2 UVflip = round(frac(float2(v.texcoord0.z * 13, v.texcoord0.z * 8))); 	//random 0 or 1 in x and y
					UVflip = UVflip * 2 - 1; 														//random -1 or 1 in x and y
					UVflip = lerp(1, UVflip, float2(_FlipU, _FlipV));
					
					#ifdef SHADER_API_GLES3
						fallPos *= -1.0;
					#endif
					
					float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz + fallPos;


					o.vertex = UnityObjectToClipPos(v.vertex) + fallPos;
					o.color = v.color;
					o.color.a *= o.color.a;
					o.color.a += _AlphaMin;
					o.normal = UnityObjectToWorldNormal(v.normal);
					o.customData = float3(v.texcoord1.xy, v.texcoord0.z);
					UNITY_TRANSFER_FOG(o,o.vertex);

					// o.uv.xy is original UVs, o.uv.zw is randomized and panned //
					o.uv.xy = TRANSFORM_TEX(v.texcoord0.xy * UVflip, _MainTex);
					o.uv.zw = o.uv.xy * half2(_Columns, _Rows) + v.texcoord0.z * half2(3,8) * _Randomize;
					
					#ifdef SPECULAR_REFLECTION_ON
						// get all the vectors and matricies I need to handle normalmapped reflections //
						float3 binormal = cross(v.normal, v.tangent.xyz) * v.tangent.w;								
						float3x3 rotation = float3x3(v.tangent.xyz, binormal, v.normal);							
						o.tangentToWorld = mul((float3x3)unity_ObjectToWorld, transpose(rotation));							
						float3 worldViewDir = normalize (UnityWorldSpaceViewDir(worldPos));							
						o.viewDir = worldViewDir;																	
					#endif

					//Do vertex lighting
					float3 shade = ShadeSH9(float4(o.normal,1));
					shade = max(shade, (unity_AmbientSky + unity_AmbientGround + unity_AmbientEquator) * 0.15);		//Don't go to 0 even if there's no significant lighting data
					o.vertLight.xyz = lerp(1, shade, _LightStr);
					//vertlight.w is currently unused

					return o;
				}


				////// FRAGMENT PROGRAM //////
				half4 frag (v2f i) : SV_Target
				{	
					
					
					////// Sample The UV Offset //////
					float4 uvWarp = tex2D(_WarpTex, i.uv.zw * _WarpTex_ST.xy + _WarpTex_ST.zw * (i.customData.x + 1) + (float2(5,8) * i.customData.z) );
					float2 warp = (uvWarp.xy * 2) - 1;
					warp *= _WarpStr * i.customData.y;

					////// Sample The Mask //////
					half4 mask = tex2D(_MainTex, i.uv.xy * _MainTex_ST.xy + warp);
					mask = saturate(lerp(1, mask, _MaskStr));

					////// Make And Edge Mask So Nothing Spills Off The Quad //////
					half2 tempUV = frac(i.uv.xy * half2(_Columns, _Rows)) - 0.5;
					tempUV *= tempUV * 4;
					half edgeMask = saturate(tempUV.x + tempUV.y);
					edgeMask *= edgeMask;
					edgeMask = 1- edgeMask;
					edgeMask = lerp(1.0, edgeMask, _ProcMask);

					mask *= edgeMask;
					half4 col = max(0.001, i.color);
					col.a = saturate(dot(mask, _ChannelMask));



					////// Sample The Noise //////
					half4 noise4 = tex2D(_NoiseTex, i.uv.zw * _NoiseTex_ST.xy + _NoiseTex_ST.zw * i.customData.x + warp);
					half noise = dot(noise4, _ChannelMask2);
					noise = saturate(lerp(1,noise,_NoiseAlphaStr));

					////// Alpha Clip //////
					col.a *= noise;
					half preClipAlpha = col.a;
					half clippedAlpha =  saturate((preClipAlpha * i.color.a - _AlphaMin)/(_AlphaSoft));
					col.a = clippedAlpha;

					////// Bring In Base Lighting //////
					float3 baseLighting = max(0.01,(i.vertLight + 0.2 * dot(i.vertLight, half3(1,1,1))));
					baseLighting = i.vertLight.xyz;
					
					#ifdef SPECULAR_REFLECTION_ON
						////// Sample The Normals //////
						half3 normalTex = UnpackNormal(tex2D(_Normal, i.uv.zw * _NoiseTex_ST.xy + _NoiseTex_ST.zw * i.customData.x + warp));

						////// Make Normals Steep Near Alpha Edge //////
						normalTex.z = _FlattenNormal * (preClipAlpha + preClipAlpha + col.a - 1) * 0.5;
						normalTex.z = _FlattenNormal * (saturate((preClipAlpha * i.color.a - _AlphaMin)/(_AlphaSoft + 0.2)) - 0.1) * 1.2;
						normalTex = normalize(normalTex);

						////// Transform Normals To World Space //////
						normalTex.xyz = mul(i.tangentToWorld, normalTex.xyz);
						float3 combinedNormals = normalize(i.normal + normalTex);
						float3 viewDir = (combinedNormals + half3(0,1,0)) * 0.5;

						////// Calculate Reflection UVs ///////
						float3 reflectionVector = reflect(-i.viewDir, combinedNormals);
						reflectionVector.x = atan2(reflectionVector.x, reflectionVector.z) * 0.31831;
						reflectionVector = reflectionVector * 0.5;
						float2 reflectionUVs = reflectionVector.xy * _ReflectionTex_ST.xy;
						reflectionUVs += _ReflectionTex_ST.zw * (_Time + i.customData.z);
						float3 reflectionTex = tex2D(_ReflectionTex, reflectionUVs);

						////// Generate Specular Reflection//////
						float desatReflection = dot(reflectionTex, float3(1,1,1)) * 0.333;
						float3 spec = lerp(desatReflection, reflectionTex, _ReflectionSat);
						float3 spec0 = spec;
						float3 spec1 = spec0 * spec0 * spec0 * spec0;
						spec = clamp(lerp(spec0, spec1, _SpecularColor.w * preClipAlpha),0,10);


						float fresnel = 1 - dot(i.viewDir, combinedNormals) * _SpecularColor.w;
						spec *= clamp(fresnel, 0.2,1);
		
					#endif
					

					////// Find Edge //////
					half edge = 1 - saturate(preClipAlpha * clippedAlpha);
					edge *= edge;
					edge = 1 - edge;
					edge = edge + lerp(0, noise - 0.5, _NoiseColorStr);

					////// Edge Darken //////
					edge = saturate(lerp(0.71, edge * edge, _EdgeDarken));

					////// Edge Alpha //////
					col.a *= saturate(lerp(1.25, _BaseColor.a , edge));
					
					#ifdef SPECULAR_REFLECTION_OFF
						edge *= 2;
					#endif 

					col.xyz *= lerp(min(col.xyz * col.xyz * col.xyz * 0.3, 1.0), 0.71, edge);  /// Make sure this doesn't end up wAAAAAAY over one


					////// Tint And Combine Lighting //////

					col.xyz *= max(0,baseLighting * _BaseColor.xyz);
					
					#ifdef SPECULAR_REFLECTION_ON
						col.xyz += baseLighting * spec * _SpecularColor.xyz;
					#endif

					///// Apply Fog //////
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG
			}

		}

		////// THIS IS A FALLBACK FOR SHADER MODEL 2.0 and OPENGL ES 2.0 DEVICES //////
		////// UV warping, noise panning, specular reflections, vertex animation, and vertex lighting are disabled for performance and hardware limitations //////

		SubShader {
			Pass {
			
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				
				#include "UnityCG.cginc"

				/// Properties ///
				/// Color Controls ///
				fixed4 _BaseColor;
				fixed _AlphaMin;
				fixed _AlphaSoft;
				fixed _EdgeDarken;
				fixed _ProcMask;

				/// Mask Controls ///
				sampler2D _MainTex;
				float4 _MainTex_ST;
				fixed _MaskStr;
				fixed4 _ChannelMask;
				fixed _Columns;
				fixed _Rows;
				fixed _FlipU;
				fixed _FlipV;

				/// Noise Controls ///
				sampler2D _NoiseTex;
				float4 _NoiseTex_ST;
				fixed _NoiseAlphaStr;
				fixed _NoiseColorStr;
				fixed4 _ChannelMask2;
				fixed _Randomize;

				struct appdata_t 
				{
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float4 color : COLOR;
					float4 texcoord0 : TEXCOORD0; // Z is Random, W is Lifetime
					UNITY_VERTEX_INPUT_INSTANCE_ID

				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 uv : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					UNITY_VERTEX_OUTPUT_STEREO
				};


				v2f vert (appdata_t v)
				{
					v2f o;

					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					o.vertex = UnityObjectToClipPos(v.vertex);

					float2 UVflip = round(frac(float2(v.texcoord0.z * 13, v.texcoord0.z * 8))); 	//random 0 or 1 in x and y
					UVflip = UVflip * 2 - 1; 														//random -1 or 1 in x and y
					UVflip = lerp(1, UVflip, float2(_FlipU, _FlipV));
					
					// o.uv.xy is original UVs, o.uv.zw is randomized and panned //
					o.uv.xy = TRANSFORM_TEX(v.texcoord0.xy * UVflip, _MainTex);
					o.uv.zw = o.uv.xy * half2(_Columns, _Rows) + v.texcoord0.z * half2(3,8) * _Randomize;
					o.uv.zw *= _NoiseTex_ST.xy;
					o.uv.zw += _NoiseTex_ST.zw * v.texcoord0.w;

					o.color = v.color;
					o.color.a += _AlphaMin;

					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					
					fixed4 col = i.color;
					
					// ////// Sample The Mask //////
					fixed4 mask = tex2D(_MainTex, i.uv.xy);
					mask = saturate(lerp(1, mask, _MaskStr));

					////// Make And Edge Mask So Nothing Spills Off The Quad //////
					half2 tempUV = frac(i.uv.xy * half2(_Columns, _Rows)) - 0.5;
					tempUV *= tempUV * 4;
					half edgeMask = saturate(tempUV.x + tempUV.y);
					edgeMask *= edgeMask;
					edgeMask = 1- edgeMask;
					edgeMask = lerp(1.0,  edgeMask, _ProcMask);
					mask *= edgeMask;
					
					col.a *= saturate(dot(mask, _ChannelMask));

					
					////// Sample The Noise //////
					half4 noise4 = tex2D(_NoiseTex, i.uv.zw);
					half noise = dot(noise4, _ChannelMask2);
					noise = saturate(lerp(1,noise,_NoiseAlphaStr));

					////// Alpha Clip //////
					col.a *= noise * i.color.a;
					half preClipAlpha = col.a;
					half clippedAlpha =  saturate((preClipAlpha * i.color.a - _AlphaMin)/(_AlphaSoft));
					col.a = clippedAlpha;
					preClipAlpha = lerp(0.5, (min(preClipAlpha * 0.9 + 0.1,1.0)) * clippedAlpha, _EdgeDarken);
					col.xyz *= preClipAlpha * _BaseColor;

					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
		CustomEditor "SpecularToggleEditor"
	}
}

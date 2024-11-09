// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Effect/Add_Blend_UPR"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_ColorIntensity2("ColorIntensity", Float) = 1
		_Color2("Color", Color) = (1,1,1,1)
		[Enum(Add,1,AlphaBlend,10)]_Dst("BlendMode", Int) = 1
		[KeywordEnum(UV,UClamp,VClamp,UVClamp)] _Clamp("Clamp", Float) = 0
		[Enum(OFF,0,ON,2)]_CullMode3("CullMode", Int) = 0
		_MainTex("MainTex", 2D) = "white" {}
		_MaskIntensity1("MaskIntensity", Float) = 1
		_Mask("Mask", 2D) = "white" {}
		[Header(XY_MainTex_ZW_Mask)]_Speed1("Speed", Vector) = (0,0,0,0)
		[Toggle][Header(MainCustomData)]_Custom1xy3("Custom1xy", Float) = 0
		[Toggle][Header(MaskCustomData)]_Custom1zw1("Custom1zw", Float) = 0

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha [_Dst]
			ColorMask RGB
			Cull [_CullMode3]
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#define ASE_NEEDS_FRAG_COLOR
				#pragma shader_feature_local _CLAMP_UV _CLAMP_UCLAMP _CLAMP_VCLAMP _CLAMP_UVCLAMP


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					float4 ase_texcoord1 : TEXCOORD1;
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					float4 ase_texcoord3 : TEXCOORD3;
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform int _CullMode3;
				uniform int _Dst;
				uniform float4 _Speed1;
				uniform float _Custom1xy3;
				uniform float4 _Color2;
				uniform float _ColorIntensity2;
				uniform sampler2D _Mask;
				uniform float4 _Mask_ST;
				uniform float _Custom1zw1;
				uniform float _MaskIntensity1;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					o.ase_texcoord3 = v.ase_texcoord1;

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float2 appendResult78 = (float2(_Speed1.x , _Speed1.y));
					float2 uv_MainTex = i.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
					float2 panner79 = ( 1.0 * _Time.y * appendResult78 + uv_MainTex);
					float4 texCoord34 = i.ase_texcoord3;
					texCoord34.xy = i.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
					float2 appendResult38 = (float2(texCoord34.x , texCoord34.y));
					float2 Custom1xy42 = appendResult38;
					float2 lerpResult82 = lerp( panner79 , ( Custom1xy42 + uv_MainTex ) , _Custom1xy3);
					float2 temp_output_9_0_g1 = lerpResult82;
					float2 break12_g1 = temp_output_9_0_g1;
					float2 appendResult13_g1 = (float2(saturate( break12_g1.x ) , break12_g1.y));
					float2 break3_g1 = temp_output_9_0_g1;
					float2 appendResult1_g1 = (float2(break3_g1.x , saturate( break3_g1.y )));
					#if defined(_CLAMP_UV)
					float2 staticSwitch84 = temp_output_9_0_g1;
					#elif defined(_CLAMP_UCLAMP)
					float2 staticSwitch84 = appendResult13_g1;
					#elif defined(_CLAMP_VCLAMP)
					float2 staticSwitch84 = appendResult1_g1;
					#elif defined(_CLAMP_UVCLAMP)
					float2 staticSwitch84 = saturate( temp_output_9_0_g1 );
					#else
					float2 staticSwitch84 = temp_output_9_0_g1;
					#endif
					float4 tex2DNode59 = tex2D( _MainTex, staticSwitch84 );
					float2 appendResult41 = (float2(_Speed1.z , _Speed1.w));
					float2 uv_Mask = i.texcoord.xy * _Mask_ST.xy + _Mask_ST.zw;
					float2 panner47 = ( 1.0 * _Time.y * appendResult41 + uv_Mask);
					float2 appendResult35 = (float2(texCoord34.z , texCoord34.w));
					float2 Custom1zw37 = appendResult35;
					float2 lerpResult50 = lerp( panner47 , ( Custom1zw37 + uv_Mask ) , _Custom1zw1);
					float4 appendResult63 = (float4(( tex2DNode59 * _Color2 * _ColorIntensity2 * i.color ).rgb , ( tex2DNode59.a * _Color2.a * ( tex2D( _Mask, lerpResult50 ).r * _MaskIntensity1 ) * i.color.a )));
					

					fixed4 col = appendResult63;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.DynamicAppendNode;41;-1250.525,-202.0323;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;39;-1323.361,-367.7225;Inherit;False;0;54;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;47;-1055.82,-281.0207;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;48;-1010.881,-376.3616;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-1036.748,-149.8976;Inherit;False;Property;_Custom1zw1;Custom1zw;10;1;[Toggle];Create;True;0;0;0;False;1;Header(MaskCustomData);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;50;-778.7377,-387.0356;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-454.6133,-214.5281;Inherit;False;Property;_MaskIntensity1;MaskIntensity;6;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;60;-462.8941,-594.3391;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;-247.4102,-386.0867;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;63;97.55653,-904.5482;Inherit;True;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;-103.3528,-841.1827;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-97.02539,-664.1588;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-485.1549,-683.8524;Inherit;False;Property;_ColorIntensity2;ColorIntensity;0;0;Create;True;0;0;0;False;0;False;1;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;59;-548.5059,-1060.308;Inherit;True;Property;_MainTex;MainTex;5;0;Create;True;0;0;0;False;0;False;-1;None;d96a893e380c52848b547ab8d53d347a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;56;-499.3816,-852.9705;Inherit;False;Property;_Color2;Color;1;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;40;-1297.814,-482.3723;Inherit;False;37;Custom1zw;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;77;-1961.32,-892.8873;Inherit;False;Property;_Speed1;Speed;8;0;Create;True;0;0;0;False;1;Header(XY_MainTex_ZW_Mask);False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;78;-1625.106,-841.812;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;79;-1394.763,-1006.641;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;80;-1323.18,-859.2984;Inherit;False;Property;_Custom1xy3;Custom1xy;9;1;[Toggle];Create;True;0;0;0;False;1;Header(MainCustomData);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;81;-1346.647,-1110.83;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;82;-1140.345,-1007.255;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;83;-954.6221,-1023.281;Inherit;False;Group_UVClamp;-1;;1;13da31eea7428b244802dc8cf167af9c;0;1;9;FLOAT2;0,0;False;4;FLOAT2;23;FLOAT2;22;FLOAT2;15;FLOAT2;21
Node;AmplifyShaderEditor.StaticSwitch;84;-759.126,-1030.466;Inherit;False;Property;_Clamp;Clamp;3;0;Create;True;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;4;UV;UClamp;VClamp;UVClamp;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;85;-1568.847,-1116.917;Inherit;False;42;Custom1xy;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;86;-1690.903,-1010.221;Inherit;False;0;59;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;35;-2388.358,-1074.381;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;38;-2388.108,-1173.659;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;34;-2631.522,-1166.234;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;42;-2252.602,-1179.773;Inherit;False;Custom1xy;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;37;-2250.602,-1079.177;Inherit;False;Custom1zw;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.IntNode;64;511.0825,-634.9964;Inherit;False;Property;_CullMode3;CullMode;4;1;[Enum];Create;False;0;2;OFF;0;ON;2;0;True;0;False;0;0;False;0;1;INT;0
Node;AmplifyShaderEditor.IntNode;97;510.7754,-558.1544;Inherit;False;Property;_Dst;BlendMode;2;1;[Enum];Create;False;0;2;Add;1;AlphaBlend;10;0;True;0;False;1;10;False;0;1;INT;0
Node;AmplifyShaderEditor.SamplerNode;54;-575.73,-415.9217;Inherit;True;Property;_Mask;Mask;7;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;99;503.9681,-804.4476;Float;False;True;-1;2;ASEMaterialInspector;0;11;Effect/Add_Blend_UPR;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;True;8;5;False;;1;True;_Dst;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;True;True;2;True;_CullMode3;False;True;True;True;True;False;0;False;;False;False;False;False;False;False;False;False;False;True;2;False;;True;3;False;;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;41;0;77;3
WireConnection;41;1;77;4
WireConnection;47;0;39;0
WireConnection;47;2;41;0
WireConnection;48;0;40;0
WireConnection;48;1;39;0
WireConnection;50;0;47;0
WireConnection;50;1;48;0
WireConnection;50;2;43;0
WireConnection;57;0;54;1
WireConnection;57;1;55;0
WireConnection;63;0;62;0
WireConnection;63;3;61;0
WireConnection;62;0;59;0
WireConnection;62;1;56;0
WireConnection;62;2;58;0
WireConnection;62;3;60;0
WireConnection;61;0;59;4
WireConnection;61;1;56;4
WireConnection;61;2;57;0
WireConnection;61;3;60;4
WireConnection;59;1;84;0
WireConnection;78;0;77;1
WireConnection;78;1;77;2
WireConnection;79;0;86;0
WireConnection;79;2;78;0
WireConnection;81;0;85;0
WireConnection;81;1;86;0
WireConnection;82;0;79;0
WireConnection;82;1;81;0
WireConnection;82;2;80;0
WireConnection;83;9;82;0
WireConnection;84;1;83;23
WireConnection;84;0;83;22
WireConnection;84;2;83;15
WireConnection;84;3;83;21
WireConnection;35;0;34;3
WireConnection;35;1;34;4
WireConnection;38;0;34;1
WireConnection;38;1;34;2
WireConnection;42;0;38;0
WireConnection;37;0;35;0
WireConnection;54;1;50;0
WireConnection;99;0;63;0
ASEEND*/
//CHKSM=4521B9B571D0CAE235CD850D09CA729ACEE059C9
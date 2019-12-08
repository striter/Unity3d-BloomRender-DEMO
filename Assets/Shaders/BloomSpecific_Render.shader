﻿Shader "Hidden/PostEffect/PE_BloomSpecific_Render"
{
	SubShader
	{
		Tags { "RenderType" = "BloomColor"  "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
		UsePass "Game/Effect/BloomSpecific/Color/MAIN"
	}

	SubShader
	{
		Tags{ "RenderType" = "BloomParticlesAdditive" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
		UsePass "Game/Particle/Additive/MAIN"
	}

	SubShader
	{
		Tags{ "RenderType" = "BloomParticlesAlphaBlend" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
		UsePass "Game/Particle/AlphaBlend/MAIN"
	}

	SubShader
	{
		Tags { "RenderType" = "BloomDissolveEdge" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
		UsePass "Game/Effect/BloomSpecific/Bloom_DissolveEdge/MAIN"
	}


	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue"="Geometry"}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return float4(0,0,0,1);
			}
			ENDCG
		}
	}
}

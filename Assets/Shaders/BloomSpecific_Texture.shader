Shader "Game/Effect/BloomSpecific/Texture"
{
	Properties
	{
	    _Color("_Color",Color)=(1,1,1,1)
		_MainTex("Texture",2D)="white"{}
	}

		SubShader
	{
		Tags {"RenderType" = "BloomTexture" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Back Lighting Off ZWrite On Fog { Color(0,0,0,0) }
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			name "MAIN"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			sampler2D _MainTex;
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv:TEXCOORD1;
			};
			float4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return tex2D(_MainTex,i.uv)*_Color;
			}
			ENDCG
		}
	}
}

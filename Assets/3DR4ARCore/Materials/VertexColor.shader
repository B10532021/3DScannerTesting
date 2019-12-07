Shader "3DR4ARCore/VertexColor" {
	Properties{
	}

	SubShader
	{
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 VertexColor : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 color : COLOR;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = float4(v.VertexColor.r, v.VertexColor.g, v.VertexColor.b, 0.5f);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return float4(i.color, 0.5f);
			}
			ENDCG
		}
	}
		FallBack "Diffuse"
}

	

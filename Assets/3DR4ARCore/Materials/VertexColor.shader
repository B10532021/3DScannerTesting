Shader "3DR4ARCore/VertexColor" {
    Properties {
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = float4(v.VertexColor.r, v.VertexColor.g, v.VertexColor.b, 0.5f);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return float4(i.color, 0.5f);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"

	/*Properties{
		_Color("Color", Color) = (0,0,0,0)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_GridTex("Grid Texture", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader{
			// Tags {"RenderType"="Opaque"}
			Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Opaque"}
			LOD 200

			Pass {
				Cull Off
				Blend One OneMinusSrcAlpha
			}
			// Pass {
				ZWrite Off

				CGPROGRAM
				#pragma surface surf Standard fullforwardshadows alpha:blend
				#pragma target 3.5

				sampler2D _MainTex;
				sampler2D _GridTex;

				struct Input {
					float2 uv_MainTex;
					float4 color : COLOR;
					float3 worldPos;
				};

				half _Glossiness;
				half _Metallic;
				fixed4 _Color;

				void surf(Input IN, inout SurfaceOutputStandard o) {
					fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

					float2 gridUV = IN.worldPos.xz;
					gridUV.x *= 1 / (4 * 8.66025404);
					gridUV.y *= 1 / (2 * 15.0);
					fixed4 grid = tex2D(_GridTex, gridUV);

					o.Albedo = c.rgb * IN.color * grid;
					o.Metallic = _Metallic;
					o.Smoothness = _Glossiness;
					o.Alpha = IN.color.a;
				}
				ENDCG
					// }
		}
			FallBack "Diffuse"*/
}

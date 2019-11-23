Shader "3DR4ARCore/DepthOnly" {
    SubShader {
      Tags { "RenderType" = "Opaque" }
      ColorMask 0
      CGPROGRAM
      #pragma surface surf Lambert
      struct Input {
          float4 color : COLOR;
      };
      void surf (Input IN, inout SurfaceOutput o) {
          o.Albedo = 1;
      }
      ENDCG
    }
    FallBack "Diffuse"
}

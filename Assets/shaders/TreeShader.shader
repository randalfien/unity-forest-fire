/**
 * Simplest surface shader to show vertex color
 */

Shader "Custom/Vertex Color Surface Shader" {
    SubShader {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf Lambert
        
        struct Input {
            float4 color : COLOR;
        };
      
        void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = IN.color;
            /*
            fixed3 Emission; //possibly use to make fire glow
            */
        }
        ENDCG
    }
    Fallback "Diffuse"
  }
/**
 * Simplest surface shader to show vertex color
 */

Shader "Custom/Vertex Color Surface Shader2" {
    SubShader {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf Lambert
        
        struct Input {
            float4 color : COLOR;
        };
      
        void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = IN.color;
           // o.Emission = fixed3( 2*IN.color.r,0,0 );
            /*
            fixed3 Emission; //possibly use to make fire glow
            */
        }
        ENDCG
    }
    Fallback "Diffuse"
  }
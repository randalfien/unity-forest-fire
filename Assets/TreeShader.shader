Shader "Custom/Vertex Colored Surf Shader" {
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
                fixed3 Normal;  // tangent space normal, if written
                fixed3 Emission;
                half Specular;  // specular power in 0..1 range
                fixed Gloss;    // specular intensity
                fixed Alpha;    // alpha for transparencies
            */
        }
      ENDCG
    }
    Fallback "Diffuse"
  }
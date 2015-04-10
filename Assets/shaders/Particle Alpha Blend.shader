Shader "Particles/Alpha Blended Mine" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_BackTintColor ("Back Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_BackTex ("Back Texture", 2D) = "white" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	AlphaTest Greater .01
	ColorMask RGB
	Lighting Off ZWrite Off
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "Normal", normal
		Bind "TexCoord", texcoord
	}
	
	
	
	// ---- Dual texture cards
	SubShader {
		Pass {
			Cull Front
			
			SetTexture [_MainTex] {
				constantColor [_TintColor]
				combine constant * primary
			}
			SetTexture [_MainTex] {
				combine texture * previous DOUBLE
			}
		}
		Pass {
			Cull Back
			SetTexture [_BackTex] {
				constantColor [_BackTintColor]
				combine constant * primary
			}
			SetTexture [_BackTex] {
				combine texture * previous DOUBLE
			}
		}
	}
	
}
}

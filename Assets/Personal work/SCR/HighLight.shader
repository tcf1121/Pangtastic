Shader "Pantastic/HighLight" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
	_MainColor ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
	_OnOutline ("Outline (1=True)", Range (0, 1)) = 0
	_OutlineColor ("Outline Color", Color) = (1.0, 0.0, 0.0, 1.0)
}

 CGINCLUDE

       #include "UnityCG.cginc"

        sampler2D _MainTex;
		uniform float4 _MainTex_TexelSize;
        fixed4 _MainColor;
        fixed4 _MainTex_ST;

		fixed _OnOutline;
		fixed4 _OutlineColor;

        struct v2f {
            fixed4 pos : SV_POSITION;
            fixed2 uv : TEXCOORD0;
            fixed4 vertexColor : COLOR;
        };

        v2f vert(appdata_full v) {
            v2f o;
            o.pos = UnityObjectToClipPos (v.vertex);   
            o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
            o.vertexColor = v.color * _MainColor;
            return o; 
        }

        fixed4 frag( v2f i ) : COLOR {
				if(_OnOutline == 1)
				{
					fixed s00 = tex2D(_MainTex, i.uv + _MainTex_TexelSize * float2(-1, -1)).a;
					fixed s01 = tex2D(_MainTex, i.uv + _MainTex_TexelSize * float2( 0, -1)).a;
					fixed s02 = tex2D(_MainTex, i.uv + _MainTex_TexelSize * float2( 1, -1)).a;
					fixed s10 = tex2D(_MainTex, i.uv + _MainTex_TexelSize * float2(-1, 0)).a;
					fixed s12 = tex2D(_MainTex, i.uv + _MainTex_TexelSize * float2( 1, 0)).a;
					fixed s20 = tex2D(_MainTex, i.uv + _MainTex_TexelSize * float2(-1, 1)).a;
					fixed s21 = tex2D(_MainTex, i.uv + _MainTex_TexelSize * float2( 0, 1)).a;
					fixed s22 = tex2D(_MainTex, i.uv + _MainTex_TexelSize * float2( 1, 1)).a;

					fixed sobelX = s00 + 2 * s10 + s20 - s02 - 2 * s12 - s22;
					fixed sobelY = s00 + 2 * s01 + s02 - s20 - 2 * s21 - s22;

					fixed edgeSqr = (sobelX * sobelX + sobelY * sobelY);

					_OutlineColor.a = edgeSqr;

					if(tex2D (_MainTex, i.uv.xy).a < 0.5) return _OutlineColor;
				}

                return tex2D (_MainTex, i.uv.xy) * i.vertexColor;
        }

    ENDCG

SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha

    Pass {
        CGPROGRAM

        #pragma vertex vert
        #pragma fragment frag
        #pragma fragmentoption ARB_precision_hint_fastest 

        ENDCG
        }
    } 
    FallBack Off
}
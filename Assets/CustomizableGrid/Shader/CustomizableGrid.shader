Shader "Zaroa/CustomizableGrid" {
	Properties {
		_BaseColor ("Base Color", Color) = (0.5,0.5,0.5,1)
		_TilingX ("Global Tiling X", Float) = 1
		_TilingY ("Global Tiling Y", Float) = 1
		[Toggle] _WorldPosition("World Position UV", Float) = 0

		_GlobalMet("Global Metallic", Range(-1, 2)) = 1
		_GlobalSmo("Global Smoothness", Range(-1, 2)) = 1
		_GlobalEmi("Global Emission", Range(0, 2)) = 1

		_BasePatternColor ("Base Pattern Color", Color) = (0.8,0.8,0.8,1)
		_BasePatternTex ("Base Patter Texture", 2D) = "black" {}
		_BasePatternMet ("Metallic BP", Range (-1, 1)) = 0
		_BasePatternSmo ("Smoothness BP", Range (0, 1)) = 0.3
		_BasePatternEmi ("Emission BP", Range (0, 10)) = 0

		_CenterGridColor ("Center Grid Color", Color) = (0.7,0.7,0.7,1)
		_CenterGridTex ("Center Grid Texture", 2D) = "black" {}
		_CenterGridMet ("Metallic CG", Range (-1, 1)) = 0
		_CenterGridSmo ("Smoothness CG", Range (0, 1)) = 0.3
		_CenterGridEmi ("Emission CG", Range (0, 10)) = 0

		_CenterLineColor ("Center Line Color", Color) = (0.6,0.6,0.6,1)
		_CenterLineTex ("Center Line Texture", 2D) = "black" {}
		_CenterLineMet ("Metallic CL", Range (-1, 1)) = 0
		_CenterLineSmo ("Smoothness CL", Range (0, 1)) = 0.3
		_CenterLineEmi ("Emission CL", Range (0, 10)) = 0

		_EdgeColor ("Edge Color", Color) = (0.9,0.9,0.9,1)
		_EdgeTex ("Edge Texture", 2D) = "black" {}
		_EdgeMet ("Metallic Edge", Range (-1, 1)) = 0
		_EdgeSmo ("Smoothness Edge", Range (0, 1)) = 0.3
		_EdgeEmi ("Emission Edge", Range (0, 10)) = 0

		_TextColor ("Text Color", Color) = (0.9,0.9,0.9,1)
		_TextTex ("Text Texture", 2D) = "black" {}
		_TextMet ("Metallic Text", Range (-1, 1)) = 0
		_TextSmo ("Smoothness Text", Range (0, 1)) = 0.3
		_TextEmi ("Emission Text", Range (0, 10)) = 0
	
		_WolrdBpattTiling("BP WTiling", Vector) = (1,1,0,0)
		_WolrdCgridTiling("CG WTiling", Vector) = (1,1,0,0)
		_WolrdClineTiling("CL WTiling", Vector) = (1,1,0,0)
		_WolrdEdgeTiling("Edge Tiling", Vector) = (1,1,0,0)
		_WolrdTextTiling("Text Tiling", Vector) = (1,1,0,0)


	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		#pragma target 4.0
		#pragma multi_compile WPOS_ON WPOS_OFF

		fixed4 _BaseColor;

		fixed4 _BasePatternColor;
		sampler2D _BasePatternTex;
		half _BasePatternMet;
		half _BasePatternSmo;
		half _BasePatternEmi;

		fixed4 _CenterGridColor;
		sampler2D _CenterGridTex;
		half _CenterGridMet;
		half _CenterGridSmo;
		half _CenterGridEmi;

		fixed4 _CenterLineColor;
		sampler2D _CenterLineTex;
		half _CenterLineMet;
		half _CenterLineSmo;
		half _CenterLineEmi;

		fixed4 _EdgeColor;
		sampler2D _EdgeTex;
		half _EdgeMet;
		half _EdgeSmo;
		half _EdgeEmi;

		fixed4 _TextColor;
		sampler2D _TextTex;
		half _TextMet;
		half _TextSmo;
		half _TextEmi;

		struct Input {
			float2 uv_BasePatternTex;
			float2 uv_CenterGridTex;
			float2 uv_CenterLineTex;
			float2 uv_EdgeTex;
			float2 uv_TextTex;

			float3 worldNormal;
			float3 worldPos;
		};

		half _TilingX;
		half _TilingY;

		half _GlobalMet;
		half _GlobalSmo;
		half _GlobalEmi;

		half4 _WolrdBpattTiling;
		half4 _WolrdCgridTiling;
		half4 _WolrdClineTiling;
		half4 _WolrdEdgeTiling;
		half4 _WolrdTextTiling;

		bool _WorldPosition;

		void surf (Input IN, inout SurfaceOutputStandard o) {

			float2 overT = float2(_TilingX,_TilingY);

			float2 UV_BP = IN.uv_BasePatternTex * overT;
			float2 UV_CG = IN.uv_CenterGridTex * overT;
			float2 UV_CL = IN.uv_CenterLineTex * overT;
			float2 UV_ED = IN.uv_EdgeTex * overT;
			float2 UV_TX = IN.uv_TextTex * overT;
#if WPOS_ON
				
			if (abs(IN.worldNormal.x)>0.5) {// side
				UV_BP = (IN.worldPos.zy * overT * _WolrdBpattTiling.xy) + _WolrdBpattTiling.zw;
				UV_CG = (IN.worldPos.zy * overT * _WolrdCgridTiling.xy) + _WolrdCgridTiling.zw;
				UV_CL = (IN.worldPos.zy * overT * _WolrdClineTiling.xy) + _WolrdClineTiling.zw;
				UV_ED = (IN.worldPos.zy * overT * _WolrdEdgeTiling.xy) + _WolrdEdgeTiling.zw;
				UV_TX = (IN.worldPos.zy * overT * _WolrdTextTiling.xy) + _WolrdTextTiling.zw;
					
			}
			else if (abs(IN.worldNormal.z)>0.5) {// front
				UV_BP = (IN.worldPos.xy * overT * _WolrdBpattTiling.xy) + _WolrdBpattTiling.zw;
				UV_CG = (IN.worldPos.xy * overT * _WolrdCgridTiling.xy) + _WolrdCgridTiling.zw;
				UV_CL = (IN.worldPos.xy * overT * _WolrdClineTiling.xy) + _WolrdClineTiling.zw;
				UV_ED = (IN.worldPos.xy * overT * _WolrdEdgeTiling.xy) + _WolrdEdgeTiling.zw;
				UV_TX = (IN.worldPos.xy * overT * _WolrdTextTiling.xy) + _WolrdTextTiling.zw;
			}
			else { // top
				UV_BP = (IN.worldPos.xz * overT * _WolrdBpattTiling.xy) + _WolrdBpattTiling.zw;
				UV_CG = (IN.worldPos.xz * overT * _WolrdCgridTiling.xy) + _WolrdCgridTiling.zw;
				UV_CL = (IN.worldPos.xz * overT * _WolrdClineTiling.xy) + _WolrdClineTiling.zw;
				UV_ED = (IN.worldPos.xz * overT * _WolrdEdgeTiling.xy) + _WolrdEdgeTiling.zw;
				UV_TX = (IN.worldPos.xz * overT * _WolrdTextTiling.xy) + _WolrdTextTiling.zw;
			}

#endif
			fixed4 colBaseP = tex2D (_BasePatternTex, UV_BP);
			fixed4 colCGrid = tex2D (_CenterGridTex, UV_CG);
			fixed4 colCLine = tex2D (_CenterLineTex, UV_CL);
			fixed4 colEdge = tex2D (_EdgeTex, UV_ED);
			fixed4 colText = tex2D (_TextTex, UV_TX);

			fixed4 colFinal =  ((((_BaseColor * (1 - colBaseP) + (colBaseP * _BasePatternColor)) * 
								(1 - colCGrid) + (colCGrid * _CenterGridColor)) * 
								(1 - colCLine) + (colCLine * _CenterLineColor)) * 
								(1 - colEdge) + (colEdge * _EdgeColor)) * 
								(1 - colText) + (colText * _TextColor);

			fixed4 metBaseP = _BasePatternMet > 0 ? _BasePatternMet * colBaseP : - _BasePatternMet * (1 - colBaseP);
			fixed4 metCGrid = _CenterGridMet > 0 ? _CenterGridMet * colCGrid : - _CenterGridMet * (1 - colCGrid);
			fixed4 metCLine = _CenterLineMet > 0 ? _CenterLineMet * colCLine : - _CenterLineMet * (1 - colCLine);
			fixed4 metEdge = _EdgeMet > 0 ? _EdgeMet * colEdge : - _EdgeMet * (1 - colEdge);
			fixed4 metText = _TextMet > 0 ? _TextMet * colText : - _TextMet * (1 - colText);

			fixed4 metFinal = metBaseP + metCGrid + metCLine + metEdge + metText;
			fixed4 smoFinal = (metBaseP * _BasePatternSmo) + (metCGrid * _CenterGridSmo) + (metCLine * _CenterLineSmo) +
						(metEdge * _EdgeSmo) + ( metText * _TextSmo);

			fixed4 emiFinal = (colBaseP * _BasePatternColor * _BasePatternEmi) +
					(colCGrid * _CenterGridColor * _CenterGridEmi) + 
					(colCLine * _CenterLineColor * _CenterLineEmi) + 
					(colEdge * _EdgeColor * _EdgeEmi) + 
					(colText * _TextColor * _TextEmi);

			o.Albedo = colFinal.rgb;

			o.Metallic = metFinal * _GlobalMet;

		    o.Smoothness = smoFinal * _GlobalSmo;
		    o.Emission = emiFinal * _GlobalEmi;

			o.Alpha = 1;
		}
		ENDCG
	}
	CustomEditor "CustomizableGridShaderGUI"
	FallBack "Diffuse"
}

Shader "Custom/MaskShader2D"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _HighlightColour("Colour Highlight", Color) = (1,1,1,1)
        _IsCorrectPosition("Correct Position", Int) = 0
        _DesaturationAmount("Desaturation Percentage", Range(0, 1)) = 0.6
        }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100
        //Quads winding order is sometimes off, so turn off quads.
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

           
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 col : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 col : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _HighlightColour;
            int _IsCorrectPosition;
            float _DesaturationAmount;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.col = v.col;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //Sample the Render texture.
                fixed4 col = tex2D(_MainTex, i.uv);

                //Multiply by the Highlight Colour

                fixed4 desatColour = dot(col.rgb, float3(0.299, 0.587, 0.114));
                //Multiply by desaturation if desaturation has been turned on.
                fixed3 colDesat = lerp(col.rgb, desatColour, _DesaturationAmount);
                
                //If in the correct position, use the desaturated colour (If applicable).
                col.rgb = (_IsCorrectPosition == 1 ? col.rgb : colDesat);

                col = col * _HighlightColour * i.col;

                return col;
            }
            ENDCG
        }
    }
}
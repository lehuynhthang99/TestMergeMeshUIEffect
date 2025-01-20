Shader "UI/GlowEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        _MulFactor("MulFactor", float) = 0.15
        _AddRange("AddRange", Range(0, 1)) = 0.1

    }
    SubShader
    {
        Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work

            #include "UnityCG.cginc"

            struct appdata
            {
                half4 vertex : POSITION;
                half2 uv : TEXCOORD0;
                half4 color    : COLOR;
            };

            struct v2f
            {
                half2 uv : TEXCOORD0;
                half4 vertex : SV_POSITION;
                half4 color : TEXCOORD1;
            };

            half _MulFactor;
            half _AddRange;
            
            sampler2D _MainTex;

            half when_eq(half x, half y) 
            {
                return 1.0 - abs(sign(x - y));
            }


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }



            fixed4 frag(v2f i) : SV_Target
            {
                half4 mapColor = tex2D(_MainTex, i.uv);

                

                half factor = max(mapColor.a - clamp( ((sin(_Time.y) + 0) * 4.0 ) - 2.0, 0.0, 1.0) , 0);
                // half factor = min(clamp( ((sin(_Time.y * 4.0) + 1) * 2.0 ), 0.0, 1.0) - mapColor.a, 1);

                half4 resultColor = i.color;

                resultColor.a = factor;

                return resultColor;
            }
            ENDCG
        }
    }
}
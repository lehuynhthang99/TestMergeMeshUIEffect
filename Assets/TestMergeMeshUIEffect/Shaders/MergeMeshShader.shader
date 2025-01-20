Shader "UI/MergeMeshShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        _MulFactor("MulFactor", float) = 0.15
        _CompareColor("CompareColor", Color) = (1,1,1,1)

        _AddColor("AddColor", Color) = (1,1,1,1)
        _MulColor("MulColor", Color) = (1,1,1,1)

    }
    SubShader
    {
        Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

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
            half4 _CompareColor;
           
            half4 _AddColor;
            half4 _MulColor;
            
            sampler2D _MainTex;

            // half4 invLerp(half4 from, half4 to, half4 value) 
            // {
            //     return (value - from) / (to - from);
            // }

            // half my_smoothstep(half x)
            // {
            //     return x * x * (3.0 - 2.0 * x);
            // }

            
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

                half delta = mapColor.r - _CompareColor.r;
                half signOfDelta = sign(delta);

                half4 resultColor = when_eq(0, signOfDelta) * i.color +
                                    when_eq(1, signOfDelta) * (i.color + delta * _AddColor) +
                                    when_eq(-1, signOfDelta) * (i.color - abs(delta) * _MulColor);

                // half4 resultColor = ((signOfDelta + 1) /2.0f);
                // resultColor.a = 1;

                // half4 resultColor = 0;
                // resultColor.a = 1;

                resultColor.a = mapColor.a;

                return resultColor;
            }
            ENDCG
        }
    }
}
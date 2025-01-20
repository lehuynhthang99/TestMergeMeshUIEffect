Shader "UI/OneHalfLerpMeshShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorTex ("ColorTexture", 2D) = "white" {}

        _RectBorderUV("RectBorderUV", Vector) = (1,1,1,1)

        _MulFactor("MulFactor", float) = 0.15

        _LerpRange("LerpRange", float) = 0.15

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
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            // make fog work

            #include "UnityCG.cginc"

            struct appdata
            {
                half4 vertex : POSITION;
                half2 uv : TEXCOORD0;
                float paramEff : TEXCOORD1;
                half4 color    : COLOR;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float paramEff : TEXCOORD1;
                half4 vertex : SV_POSITION;
                half4 color : COLOR;
            };

            half _MulFactor;
            half _LerpRange;
            half4 _CompareColor;

            half4 _RectBorderUV;

            half4 _TileColors[7];

            half4 _AddColor;
            half4 _MulColor;

            // half4 _DisableColor;


            #define PI 3.14159265359

            sampler2D _MainTex;
            sampler2D _ColorTex;

            half2 invLerp(half2 from, half2 to, half2 value)
            {
                return (value - from) / (to - from);
            }

            half when_lt(half x, half y)
            {
                return max(sign(y - x), 0.0);
            }

            half when_ge(half x, half y)
            {
                return 1.0 - when_lt(x, y);
            }

            half when_between(half start, half end, half value)
            {
                return when_ge(value, start) * when_lt(value, end);
            }


            half when_eq(half x, half y)
            {
                return 1.0 - abs(sign(x - y));
            }

            float2 UnpackUV(float input)
            {
                float2 result;
                result.x = (input % 10000) / 8192;
                result.y = (input / 10000) / 8192;

                return result;
            }

            void UnpackParamEff(float input, out half4 color1, out half4 color2)
            {
                int index = round(input % 100);
                color2 = _TileColors[index];
                input /= 100;

                index = round(input % 100);
                color1 = _TileColors[index];
            }


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = UnpackUV(v.uv.x);
                o.uv.zw = UnpackUV(v.uv.y);
                o.paramEff = v.paramEff;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                
                half2 locationUV = invLerp(half2(_RectBorderUV.xy), half2(_RectBorderUV.zw), i.uv.zw)* 2.0 - 1.0;

                half angle = (atan2(locationUV.y, locationUV.x) - PI/6.0 + _Time.y + (2.0 * PI)) % (2.0 * PI);

                half4 color1, color2, color3;
                half isEnable;
                UnpackParamEff(i.paramEff, color1, color2);

                half4 outColor = color1;

                half4 startColor = color1;
                half4 endColor = color2;
                half startAngle = 0;
                half endAngle = PI;
                half centerValue = (startAngle + endAngle) / 2.0;
                half factorLerp = smoothstep(centerValue - _LerpRange, centerValue + _LerpRange, angle);
                half factorPos = when_between(startAngle, endAngle, angle);
                half4 lerpColor = (1 - factorLerp) * startColor + factorLerp * endColor;
                outColor = (1 - factorPos) * outColor + factorPos * lerpColor;

                startColor = color2;
                endColor = color1;
                startAngle = PI;
                endAngle = 2.0 * PI;
                centerValue = (startAngle + endAngle) / 2.0;
                factorLerp = smoothstep(centerValue - _LerpRange, centerValue + _LerpRange, angle);
                factorPos = when_between(startAngle, endAngle, angle);
                lerpColor = (1 - factorLerp) * startColor + factorLerp * endColor;
                outColor = (1 - factorPos) * outColor + factorPos * lerpColor;

                half4 mapColor = tex2D(_MainTex, i.uv.xy);

                half delta = mapColor.r - _CompareColor.r;
                half signOfDelta = sign(delta);

                half4 resultColor = when_eq(0, signOfDelta) * outColor +
                                    when_eq(1, signOfDelta) * (outColor + delta * _AddColor) +
                                    when_eq(-1, signOfDelta) * (outColor - abs(delta) * _MulColor);

                resultColor.a = mapColor.a;

                return resultColor * i.color;
            }
            ENDHLSL
        }
    }
}
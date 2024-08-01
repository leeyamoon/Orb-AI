Shader "Hidden/InverseCircleS"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BallLocation("Ball Location", Vector) = (0.5,0.5,0,0)
        _Radius("Radius", Float) = 0.5
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float2 _BallLocation;
            float _Radius;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 normalizedCoords = i.uv - _BallLocation;
                normalizedCoords.x *= _ScreenParams.x / _ScreenParams.y;
                float distanceToBall = length(normalizedCoords);
                if (distanceToBall > _Radius)
                {
                    col.rgb = 1 - round(col.r);
                }
                return col;
            }
            ENDCG
        }
    }
}

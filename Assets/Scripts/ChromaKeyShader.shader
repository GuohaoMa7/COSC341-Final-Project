Shader "Custom/ChromaKeySpriteShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _ChromaKeyColor ("Chroma Key Color", Color) = (0, 1, 0, 1) // Green
        _Tolerance ("Tolerance", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            fixed4 _ChromaKeyColor;
            float _Tolerance;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.texcoord);

                // Calculate the difference between the texture color and the chroma key color
                float3 diff = abs(texColor.rgb - _ChromaKeyColor.rgb);

                // Calculate transparency based on the tolerance
                float alpha = step(_Tolerance, dot(diff, float3(0.3333, 0.3333, 0.3333)));

                return fixed4(texColor.rgb, texColor.a * alpha);
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}

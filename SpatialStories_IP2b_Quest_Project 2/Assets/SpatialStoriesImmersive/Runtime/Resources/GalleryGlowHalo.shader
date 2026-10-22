Shader "SpatialStoriesImmersive/GalleryGlowHalo"
{
    Properties
    {
        _Color ("Light colour and strength", Color) = (.24,.87,.73,.3)
        _Shape ("0: ring, 1: strip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            fixed4 _Color;
            half _Shape;
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f,o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half2 p = i.uv*2-1;
                half radius = length(p);
                half ring = smoothstep(.36h,.59h,radius)*(1-smoothstep(.59h,1.0h,radius));
                half strip = pow(saturate(1-abs(p.y)),3)*(1-smoothstep(.82h,1.0h,abs(p.x)));
                half alpha = lerp(ring,strip,saturate(_Shape))*_Color.a;
                return fixed4(_Color.rgb,alpha);
            }
            ENDCG
        }
    }
}

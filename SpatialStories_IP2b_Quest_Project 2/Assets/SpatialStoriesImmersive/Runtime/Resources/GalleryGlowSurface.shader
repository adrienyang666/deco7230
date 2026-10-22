Shader "SpatialStoriesImmersive/GalleryGlowSurface"
{
    Properties
    {
        _Color ("Body colour", Color) = (.24,.87,.73,1)
        _RimColor ("Edge light", Color) = (.6,1,.9,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
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
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 world : TEXCOORD0;
                half3 normal : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            fixed4 _Color, _RimColor;
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half3 n = normalize(i.normal);
                half3 view = normalize(UnityWorldSpaceViewDir(i.world));
                half3 light = normalize(half3(-.45,.8,-.6));
                half facing = saturate(dot(n,view));
                half rim = pow(1-facing,3);
                half shine = pow(saturate(dot(n,normalize(light+view))),40);
                half shade = .45h + .42h*saturate(dot(n,light));
                // A stable, pearly highlight without extra lights or a screen-wide effect.
                half3 colour = _Color.rgb*shade + _RimColor.rgb*rim*.52h + shine*.38h;
                return fixed4(saturate(colour),1);
            }
            ENDCG
        }
    }
    Fallback "Unlit/Color"
}

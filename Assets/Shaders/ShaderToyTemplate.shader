// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Shadertoy/Template" {
    Properties{
        //PROPERTIES
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    #pragma target 3.0

    #define vec2 float2
    #define vec3 float3
    #define vec4 float4
    #define mat2 float2x2
    #define mat3 float3x3
    #define mat4 float4x4
    #define iTime _Time.y
    #define iGlobalTime _Time.y
    #define mod fmod
    #define mix lerp
    #define fract frac
    #define refrac refract
    #define texture2D tex2D
    #define iResolution _ScreenParams
    #define gl_FragCoord ((_iParam.scrPos.xy/_iParam.scrPos.w) * _ScreenParams.xy)

    #define PI2 6.28318530718
    #define pi 3.14159265358979
    #define halfpi (pi * 0.5)
    #define oneoverpi (1.0 / pi)

    float atan(float x, float y) {
        return atan2(y, x);
    }

    //VARIABLES

    struct v2f {
        float4 pos : SV_POSITION;
        float4 scrPos : TEXCOORD0;
    };
    /*
    * struct appdata_base {
        float4 vertex : POSITION;　　//顶点位置
        float3 normal : NORMAL;　　//法线
        float4 texcoord : TEXCOORD0;//纹理坐标
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    */
    v2f vert(appdata_base v) {
        v2f o;
        o.pos = UnityObjectToClipPos (v.vertex);
        o.scrPos = ComputeScreenPos(o.pos);
        return o;
    }

    vec4 mainImage(vec2 fragCoord);

    fixed4 frag(v2f _iParam) : SV_Target {
        vec2 fragCoord = gl_FragCoord;
        return mainImage(gl_FragCoord);
    }

    //HEADERS

    vec4 mainImage(vec2 fragCoord) {
        return vec4(1, 1, 1, 1);
    }

    ENDCG

    SubShader {
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

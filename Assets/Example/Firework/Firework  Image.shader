// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Shadertoy/Firework  Image" {
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
#define tex2D2D tex2D
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
        o.pos = UnityObjectToClipPos(v.vertex);
        o.scrPos = ComputeScreenPos(o.pos);
        return o;
    }

    vec4 mainImage(vec2 fragCoord);

    fixed4 frag(v2f _iParam) : SV_Target{
        vec2 fragCoord = gl_FragCoord;
        return mainImage(gl_FragCoord);
    }

#define NUM_PARTICLES 100.
#define NUM_EXPLOSTION 5.
        vec2 Hash12(float t) {
        float x = fract(sin(t * 455.2) * 476.5);
        float y = fract(cos(x * 89.31) * 562.15);
        return vec2(x, y);
    }
    vec2 Hash12_Polar(float t) {
        float a = fract(sin(t * 455.2) * 476.5) * 6.2832;
        float d = fract(cos((t + a) * 89.31) * 562.15);
        return vec2(sin(a), cos(a)) * d;
    }

    float Explostion(vec2 uv, float t, float size) {
        float sparks = .0;
        for (float i = 0.; i < NUM_PARTICLES; i++) {
            vec2 dir = Hash12_Polar(i + 1.) * .5;
            float d = length(uv - dir * t * size);
            float brightness = mix(0.0005, .002, smoothstep(.05, .0, t));

            if (size < 1.2)brightness *= sin(t * 20. + i) * .5 + .5;
            brightness *= smoothstep(1., .75, t);
            sparks += brightness / d;
        }
        return sparks;
    }


    vec4 mainImage(in vec2 fragCoord) {

        // Normalized pixel coordinates (from 0 to 1)
        vec2 uv = (fragCoord - .5 * iResolution.xy) / iResolution.y;

        float T = iTime;
        // Time varying pixel color
        vec3 col = vec3(.0, .0, .0);
        for (float i = .0; i < NUM_EXPLOSTION; i++) {
            float t = T + i / NUM_EXPLOSTION;
            float ft = floor(t);
            vec2 offset = Hash12(i + ft + 1.) - .5;
            vec3 color = sin(vec3(.34, .54, .22) * ft * 4. + i * 10.) * .25 + .75;
            float size = offset.x + offset.y + 1.;
            col += Explostion(uv - offset, fract(t), size) * color;
        }

        //col *= 2.;

        // Output to screen
        return vec4(col, 1.0);

    }

    ENDCG

        SubShader{
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

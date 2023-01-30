// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Shadertoy/Plasma" { 
    Properties{
        iMouse ("Mouse Pos", Vector) = (100, 100, 0, 0)
        iChannel0("iChannel0", 2D) = "white" {}  
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
    #define texture2D tex2D
    #define texture tex2D
    #define iResolution _ScreenParams
    #define gl_FragCoord ((_iParam.scrPos.xy/_iParam.scrPos.w) * _ScreenParams.xy)

    #define PI2 6.28318530718
    #define pi 3.14159265358979
    #define halfpi (pi * 0.5)
    #define oneoverpi (1.0 / pi)

    fixed4 iMouse;
    
    sampler2D iChannel0;
    // sampler2D _GrabTex;

    struct v2f {    
        float4 pos : SV_POSITION;    
        float4 scrPos : TEXCOORD0;   
    };              

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

    vec4 mainImage(vec2 fragCoord) {
        vec2 vp = vec2(320.0, 200.0);
        float t = iTime * 10.0 + iMouse.x;
        vec2 uv = fragCoord.xy / iResolution.xy;
        vec2 p0 = (uv - 0.5) * vp;
        vec2 hvp = vp * 0.5;
        vec2 p1d = vec2(cos(t / 98.0),  sin(t / 178.0)) * hvp - p0;
        vec2 p2d = vec2(sin(-t / 124.0), cos(-t / 104.0)) * hvp - p0;
        vec2 p3d = vec2(cos(-t / 165.0), cos( t / 45.0))  * hvp - p0;
        float sum = 0.5 + 0.5 * (
            cos(length(p1d) / 30.0) +
            cos(length(p2d) / 20.0) +
            sin(length(p3d) / 25.0) * sin(p3d.x / 20.0) * sin(p3d.y / 15.0));
        return texture(iChannel0, vec2(fract(sum), 0));
    }

    ENDCG

    SubShader {
        //_GrabTexture works too in OnPostRender
//        GrabPass {"_GrabTex"}
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

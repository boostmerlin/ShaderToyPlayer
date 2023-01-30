// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Shadertoy/TextureLOD" { 
    Properties{
        iChannel0("Start Point", 2D) = "white" {}
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
    #define iGlobalTime _Time.y
    #define iTime _Time.y
    #define mod fmod
    #define mix lerp
    #define fract frac
    #define texture2D tex2D
    #define iResolution _ScreenParams
    #define gl_FragCoord ((_iParam.scrPos.xy/_iParam.scrPos.w) * _ScreenParams.xy)

    #define PI2 6.28318530718
    #define pi 3.14159265358979
    #define halfpi (pi * 0.5)
    #define oneoverpi (1.0 / pi)

	sampler2D iChannel0;

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

    vec4 mainImage(vec2 fragCoord, v2f i);

    fixed4 frag(v2f _iParam) : SV_Target { 
        vec2 fragCoord = gl_FragCoord;
        return mainImage(fragCoord, _iParam);
    }  

    vec4 mainImage(vec2 fragCoord, v2f i) {
        vec2 uv = fragCoord / iResolution.xy;

        float lod = (5.0 + 5.0 * sin(iTime)) * step(uv.x, 0.5);

        vec3 col = tex2Dlod(iChannel0, vec4(uv.x, 1.0 - uv.y,0,lod)).xyz;

        return vec4(col, 1.0);
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

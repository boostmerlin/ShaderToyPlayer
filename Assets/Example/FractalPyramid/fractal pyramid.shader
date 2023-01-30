// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Shadertoy/fractal pyramid" { 
    Properties{
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
        return mainImage(fragCoord);
    }

	vec3 palette(float d){
	    return mix(vec3(0.2,0.7,0.9),vec3(1.,0.,1.),d);
    }

    vec2 rotate(vec2 p,float a){
        float c = cos(a);
        float s = sin(a);
        return mul(p,mat2(c,-s,s,c));
    }
    
    float map(vec3 p){
        for( int i = 0; i<8; ++i){
            float t = iTime*0.2;
            p.xz =rotate(p.xz,t);
            p.xy =rotate(p.xy,t*1.89);
            p.xz = abs(p.xz);
            p.xz-=.5;
        }
        return dot(sign(p),p)/5.;
    }
    
    vec4 rm (vec3 ro, vec3 rd){
        float t = 0.;
        vec3 col = vec3(.0,0.,0.);
        float d;
        for(float i =0.; i<64.; i++){
            vec3 p = ro + rd*t;
            d = map(p)*.5;
            if(d<0.02){
                break;
            }
            if(d>100.){
                break;
            }
            //col+=vec3(0.6,0.8,0.8)/(400.*(d));
            col+=palette(length(p)*.1)/(400.*(d));
            t+=d;
        }
        return vec4(col,1./(d*100.));
    }
    vec4 mainImage(vec2 fragCoord)
    {
        vec2 uv = (fragCoord-(iResolution.xy/2.))/iResolution.x;
        vec3 ro = vec3(0.,0.,-50.);
        ro.xz = rotate(ro.xz,iTime);
        vec3 cf = normalize(-ro);
        vec3 cs = normalize(cross(cf,vec3(0.,1.,0.)));
        vec3 cu = normalize(cross(cf,cs));
        
        vec3 uuv = ro+cf*3. + uv.x*cs + uv.y*cu;
        
        vec3 rd = normalize(uuv-ro);
        
        vec4 col = rm(ro,rd);
        
        
        return col;
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
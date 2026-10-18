Shader "BigBattle/SequenceEnergy"
{
    Properties { _Color("Tint",Color)=(.05,1,.7,1) _Clock("Clock",Float)=0 _Progress("Progress",Float)=0 _Style("Style",Float)=0 }
    SubShader
    {
        Tags {"Queue"="Transparent+6" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct a {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
            struct v {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
            float4 _Color;float _Clock,_Progress,_Style;
            v vert(a i){v o;o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv;return o;}
            float4 frag(v i):SV_Target
            {
                float2 p=i.uv*2-1;float radius=length(p),angle=atan2(p.y,p.x);float mask=0,core=0;
                if(_Style<.5)
                {
                    float broken=.74+.26*sin(angle*7+_Clock*4);
                    mask=exp(-abs(radius-.79)*25)*broken;core=exp(-abs(radius-.79)*100);
                    mask+=exp(-abs(radius-.70)*40)*.16;
                }
                else if(_Style<1.5)
                {
                    mask=exp(-abs(radius-.86)*90)+exp(-abs(radius-.63)*70)*.6;
                    float marks=pow(saturate(cos(angle*12)),22)*smoothstep(.65,.72,radius)*(1-smoothstep(.79,.84,radius));
                    float diamonds=pow(saturate(cos(angle*6)),12)*exp(-abs(radius-.45)*38);
                    mask+=marks+diamonds;core=mask*.4;
                }
                else if(_Style<2.5)
                {
                    float rays=pow(saturate(cos(angle*6+.25)),28);
                    mask=(exp(-radius*4)*rays+exp(-radius*17))*(1-smoothstep(.75,1,radius));core=exp(-radius*20);
                }
                else if(_Style<3.5)
                {
                    float edge=abs(i.uv.y*2-1);mask=(1-smoothstep(.45,1,edge))*smoothstep(0,.08,i.uv.x)*(1-smoothstep(.82,1,i.uv.x));
                    mask*=.65+.35*sin(i.uv.x*49-_Clock*12);core=exp(-abs(i.uv.y-.58)*38);
                }
                else {mask=1;core=1-abs(i.uv.x*2-1);}
                float3 tint=lerp(_Color.rgb*.55,_Color.rgb*1.7+float3(.25,.32,.3),saturate(core));
                return float4(tint,saturate(mask)*_Color.a);
            }
            ENDCG
        }
    }
}

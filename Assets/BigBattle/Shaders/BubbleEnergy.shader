Shader "BigBattle/BubbleEnergy"
{
    Properties { _Color ("Gold tint",Color)=(1,.62,.05,1) _FxClock("Scaled clock",Float)=0 _Age("Local age",Float)=0 _Alpha("Opacity",Float)=1 _Mode("Explosion",Float)=0 }
    SubShader
    {
        Tags {"Queue"="Transparent+12" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;};
            struct v2f {float4 vertex:SV_POSITION;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;float2 uv:TEXCOORD2;};
            float4 _Color;float _FxClock,_Age,_Alpha,_Mode;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.uv=v.uv;return o;}
            float hash(float3 p){return frac(sin(dot(p,float3(12.9898,78.233,37.719)))*43758.5453);}
            float noise(float3 p){float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+1),f.x),f.y),f.z);}
            float4 frag(v2f i):SV_Target
            {
                float3 n=normalize(i.normal),view=normalize(_WorldSpaceCameraPos-i.world);float rim=pow(1-saturate(dot(n,view)),2.2);
                float light=saturate(dot(n,normalize(float3(-.6,1,-.5))));float glint=pow(light,22);
                if(_Mode<.5)
                {
                    float swirls=sin(i.uv.y*35+sin(i.uv.x*19+_FxClock*2)*1.4-_FxClock*3)*.5+.5;
                    float3 color=lerp(float3(1,.64,.065),float3(1,.97,.72),rim*.65+glint*.85);
                    color+=float3(.04,.045,.025)*swirls;return float4(color,_Alpha*(.42+rim*.55+glint*.45));
                }
                float billow=noise(i.world*2.6+float3(0,-_FxClock*4,0));float veins=smoothstep(.40,.65,billow);
                float core=saturate((1-_Age*2)*.08+veins*.15+rim*.72+glint*.55);
                float3 color=lerp(float3(1,.38,.015),float3(1,.76,.11),light*.5+veins*.5);
                color=lerp(color,float3(1,1,.88),smoothstep(.56,.88,core));
                return float4(color,_Alpha*(.80+rim*.2));
            }
            ENDCG
        }
    }
}

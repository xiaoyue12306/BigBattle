Shader "BigBattle/DoomEnergy"
{
    Properties { _Ground ("Ground", Float)=0 _Clock ("Clock",Float)=0 _Opacity ("Opacity",Float)=1 }
    SubShader
    {
        Tags { "Queue"="Transparent+2" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
            struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
            float _Ground,_Clock,_Opacity;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
            float4 frag(v2f i):SV_Target
            {
                float u=i.uv.x,v=i.uv.y;
                float jag=sin(v*79+sin(v*31)*3)*.024+sin(v*173)*.011;
                float band=abs(frac(u*4+jag)-.5)*2;
                float edge=pow(saturate(band),9);
                float vein=pow(saturate(1-abs(sin(v*34+u*19+sin(v*91)*.6-_Clock*4))),18);
                float pulse=.8+.2*sin(_Clock*17-v*23);
                float3 col=float3(.006,.014,.025)+float3(.02,1.8,1.08)*edge*pulse;
                col+=float3(.03,.37,1.6)*vein*.7;
                col+=float3(1.2,.015,.07)*pow(saturate(sin(v*56-u*29+_Clock*6)),38)*.45;
                float alpha=.96;
                if(_Ground>.5)
                {
                    float across=abs(u*2-1);
                    float rim=exp(-abs(across-.40+jag)*17);
                    float heart=exp(-across*7);
                    col=float3(.012,.028,.038)+float3(.015,1.8,.85)*rim*pulse+float3(.02,.5,1.8)*heart*(.7+.3*sin(v*16-_Clock*9));
                    col+=float3(.9,.01,.06)*vein*.22;
                    alpha=(1-smoothstep(.82,1,across))*.97;
                    if(_Ground>1.5){col=float3(.025,.045,.052);alpha=(1-smoothstep(.3,1,across))*.68;}
                }
                return float4(col,alpha*_Opacity);
            }
            ENDCG
        }
    }
}

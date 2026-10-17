Shader "BigBattle/SoulParticle"
{
    Properties
    {
        _MainTex ("Kenney CC0 particle", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _DstBlend ("Destination blend", Float) = 1
        _Glow ("Bright core", Float) = 1
        _Clock ("Paused effect clock", Float) = 0
        _Flow ("Flame refraction within sprite", Float) = 0
        _UvRotate ("Ribbon texture orientation", Float) = 0
        _Sheet ("Columns rows current next frame", Vector) = (1,1,0,0)
        _FrameBlend ("Frame interpolation", Float) = 0
        _Gray ("Recolour source energy", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+5" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha [_DstBlend]
        Cull Off ZWrite Off Lighting Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;float4 _Color,_Sheet;float _Clock,_Flow,_Glow,_UvRotate,_FrameBlend,_Gray;
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
            float2 frameUV(float2 uv,float index)
            {
                float2 tile=float2(fmod(index,_Sheet.x),_Sheet.y-1-floor(index/_Sheet.x));return (uv+tile)/_Sheet.xy;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float2 uv=lerp(i.uv,i.uv.yx,_UvRotate);uv.x+=sin(uv.y*19-_Clock*5)*_Flow*sin(uv.y*3.14159);
                fixed4 tex=tex2D(_MainTex,frameUV(uv,_Sheet.z));
                if(_Sheet.x>1)tex=lerp(tex,tex2D(_MainTex,frameUV(uv,_Sheet.w)),_FrameBlend);
                tex.rgb=lerp(tex.rgb,max(tex.r,max(tex.g,tex.b)).xxx,_Gray);
                float edge=saturate(min(min(uv.x,1-uv.x),min(uv.y,1-uv.y))*25);
                fixed3 color=i.color.rgb*tex.rgb; color=lerp(color,color*.65+.35,saturate(tex.a-.72)*_Glow);
                return fixed4(color*(1+_Glow*.75),tex.a*i.color.a*edge);
            }
            ENDCG
        }
    }
}

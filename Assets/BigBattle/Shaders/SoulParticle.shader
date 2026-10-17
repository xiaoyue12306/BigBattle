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
            sampler2D _MainTex;float4 _Color;float _Clock,_Flow,_Glow,_UvRotate;
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
            fixed4 frag(v2f i):SV_Target
            {
                float2 uv=lerp(i.uv,i.uv.yx,_UvRotate);uv.x+=sin(uv.y*19-_Clock*5)*_Flow*sin(uv.y*3.14159);
                fixed4 tex=tex2D(_MainTex,uv);float edge=saturate(min(min(uv.x,1-uv.x),min(uv.y,1-uv.y))*25);
                fixed3 color=i.color.rgb*tex.rgb; color=lerp(color,color*.65+.35,saturate(tex.a-.72)*_Glow);
                return fixed4(color*(1+_Glow*.75),tex.a*i.color.a*edge);
            }
            ENDCG
        }
    }
}

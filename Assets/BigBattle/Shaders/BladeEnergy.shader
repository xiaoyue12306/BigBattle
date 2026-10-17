Shader "BigBattle/BladeEnergy"
{
    Properties
    {
        _Color ("Tint", Color) = (.15,.9,.7,1)
        _Progress ("Lifetime", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+1" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            fixed4 _Color;
            float _Progress;
            v2f vert(appdata v)
            {
                v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float across=abs(i.uv.y*2-1);
                float feather=1-smoothstep(.60,1,across);
                float core=1-smoothstep(.04,.24,abs(i.uv.y-.67));
                float filament=pow(saturate(sin(i.uv.x*85-i.uv.y*17-_Progress*14)),12)*.12;
                float lightness=max(max(_Color.r,_Color.g),_Color.b);
                float luminous=saturate((lightness-.40)*3);
                fixed3 tint=lerp(_Color.rgb*.78,fixed3(1,1,.87),(core*.91+filament)*luminous);
                float head=lerp(.38,1.25,saturate(_Progress*5));
                float reveal=1-smoothstep(head-.15,head,i.uv.x);
                return fixed4(tint,_Color.a*i.color.a*feather*reveal);
            }
            ENDCG
        }
    }
}

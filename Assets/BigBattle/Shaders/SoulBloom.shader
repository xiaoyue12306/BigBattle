Shader "Hidden/BigBattle/SoulBloom"
{
    Properties { _MainTex ("Scene",2D)="white"{} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex,_BloomTex;float2 _Direction;
        half4 prefilter(v2f_img i):SV_Target
        {
            half3 c=tex2D(_MainTex,i.uv).rgb;half brightness=max(c.r,max(c.g,c.b));
            return half4(c*saturate((brightness-1.35)/max(brightness,.001)),1);
        }
        half4 blur(v2f_img i):SV_Target
        {
            half3 c=tex2D(_MainTex,i.uv).rgb*.227027;
            c+=(tex2D(_MainTex,i.uv+_Direction*1.384615).rgb+tex2D(_MainTex,i.uv-_Direction*1.384615).rgb)*.316216;
            c+=(tex2D(_MainTex,i.uv+_Direction*3.230769).rgb+tex2D(_MainTex,i.uv-_Direction*3.230769).rgb)*.070270;
            return half4(c,1);
        }
        half4 composite(v2f_img i):SV_Target{return half4(tex2D(_MainTex,i.uv).rgb+tex2D(_BloomTex,i.uv).rgb*.32,1);}
        ENDCG
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment prefilter
            ENDCG
        }
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment blur
            ENDCG
        }
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment composite
            ENDCG
        }
    }
    Fallback Off
}

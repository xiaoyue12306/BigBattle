using UnityEngine;

namespace BigBattle
{
    // Quarter-resolution HDR bloom. The threshold leaves the pastel world alone
    // and lets overbright soul particles spill a small, soft halo around edges.
    [RequireComponent(typeof(Camera))]
    public sealed class SoulBloom : MonoBehaviour
    {
        Material material;
        void OnRenderImage(RenderTexture source,RenderTexture destination)
        {
            var shader=Shader.Find("Hidden/BigBattle/SoulBloom");
            if(shader==null || !shader.isSupported){Graphics.Blit(source,destination);return;}
            if(material==null)material=new Material(shader){hideFlags=HideFlags.HideAndDontSave};
            int w=Mathf.Max(1,source.width/4),h=Mathf.Max(1,source.height/4);
            var a=RenderTexture.GetTemporary(w,h,0,RenderTextureFormat.ARGBHalf);var b=RenderTexture.GetTemporary(w,h,0,RenderTextureFormat.ARGBHalf);
            a.filterMode=b.filterMode=FilterMode.Bilinear;
            try
            {
                Graphics.Blit(source,a,material,0);material.SetVector("_Direction",new Vector4(1f/w,0,0,0));Graphics.Blit(a,b,material,1);
                material.SetVector("_Direction",new Vector4(0,1f/h,0,0));Graphics.Blit(b,a,material,1);
                material.SetTexture("_BloomTex",a);Graphics.Blit(source,destination,material,2);
            }
            finally{RenderTexture.ReleaseTemporary(a);RenderTexture.ReleaseTemporary(b);}
        }
        void OnDestroy(){if(material!=null)Destroy(material);}
    }
}

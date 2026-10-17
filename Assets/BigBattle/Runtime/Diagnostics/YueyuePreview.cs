using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BigBattle
{
    public sealed class YueyuePreview : MonoBehaviour
    {
        [Serializable]sealed class Timing {public List<float> seconds=new List<float>();}
        GameRoot world;
        public void Setup(GameRoot game){world=game;}
        IEnumerator Start()
        {
            while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;
            world.Input.Injected=true;world.SelectHero(HeroKind.Yueyue);yield return new WaitForSecondsRealtime(.2f);
            var mage=(YueyuePlayer)world.Player;mage.transform.position=Vector3.back*3;
            for(int i=0;i<2;i++){var foe=world.SpawnEnemy(new Vector3(-9,0,5),1);foe.Damage(99,mage,Vector3.zero,0);}
            var banana=new GameObject("Recorded defeated banana").AddComponent<BananaDuelist>();banana.transform.SetParent(world.transform);banana.transform.position=new Vector3(-9,0,5);banana.SetupEnemy(world);banana.Damage(999,mage,Vector3.zero,0);
            world.Fx.Notice("越越大王 · 追魂 / 铁索 / 亡者 / 鬼影 / 万魂魔剑");
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Artifacts/YueyuePreview"));Directory.CreateDirectory(output);
            var timing=new Timing();float born=Time.unscaledTime,next=0;int index=0;
            while(Time.unscaledTime-born<11.6f)
            {
                float t=Time.unscaledTime-born;var input=new CombatInput();
                if(t<.12f)input.Move=Vector2.up;
                if(t>.3f && t<1.1f)input.Attack=true;
                if(t>1.3f && t<1.42f)input.Thrust=true;
                if(t>2.15f && t<2.27f)input.Peel=true;
                if(t>2.8f && t<2.92f)input.Dodge=true;
                if(t>3.1f && t<4.6f)input.Move=Vector2.right*.5f;
                if(t>5.9f && t<6.0f)input.Move=Vector2.up;
                if(t>6.1f && t<10.35f)input.UltimateHeld=true;
                world.Input.InjectedFrame=input;
                if(t<next){yield return null;continue;}yield return new WaitForEndOfFrame();
                Texture2D frame=ScreenCapture.CaptureScreenshotAsTexture();timing.seconds.Add(Time.unscaledTime-born);
                File.WriteAllBytes(Path.Combine(output,"frame"+index.ToString("D4")+".png"),frame.EncodeToPNG());Destroy(frame);index++;next=t+1f/15;
            }
            world.Input.InjectedFrame=default(CombatInput);File.WriteAllText(Path.Combine(output,"timing.json"),JsonUtility.ToJson(timing));
            Debug.Log("YUEYUE_PREVIEW_OK frames="+index);Application.Quit();
        }
    }
}

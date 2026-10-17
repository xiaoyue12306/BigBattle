using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BigBattle
{
    public sealed class YueyuePreview : MonoBehaviour
    {
        [Serializable]sealed class Timing {public List<float> seconds=new List<float>();public List<Vector2> actorPixels=new List<Vector2>();}
        GameRoot world;
        public void Setup(GameRoot game){world=game;}
        IEnumerator Start()
        {
            while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;
            world.Input.Injected=true;world.SelectHero(HeroKind.Yueyue);yield return new WaitForSecondsRealtime(.2f);
            var mage=(YueyuePlayer)world.Player;mage.transform.position=Vector3.back*3;world.CameraRig.CaptureCloseup=true;world.CameraRig.Snap();
            for(int i=0;i<2;i++){var foe=world.SpawnEnemy(new Vector3(-9,0,5),1);foe.Damage(99,mage,Vector3.zero,0);}
            var banana=new GameObject("Recorded defeated banana").AddComponent<BananaDuelist>();banana.transform.SetParent(world.transform);banana.transform.position=new Vector3(-9,0,5);banana.SetupEnemy(world);banana.Damage(999,mage,Vector3.zero,0);
            world.Fx.Notice("越越大王 · 追魂 / 铁索 / 亡者 / 鬼影 / 万魂魔剑");
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Artifacts/YueyuePreview"));Directory.CreateDirectory(output);
            var timing=new Timing();var frames=new System.Collections.Generic.List<Texture2D>();float born=Time.unscaledTime,next=0;int index=0;
            while(Time.unscaledTime-born<17.6f)
            {
                float t=Time.unscaledTime-born;var input=new CombatInput();
                if(t<1.8f)input.Move=new Vector2(.55f,.45f);
                if(t>=1.8f && t<3.6f)input.Move=new Vector2(-.55f,-.45f);
                if(t>3.6f && t<3.72f)input.Move=Vector2.up;
                if(t>4 && t<5.5f)input.Attack=true;
                if(t>5.65f && t<5.77f)input.Thrust=true;
                if(t>6.6f && t<6.72f)input.Peel=true;
                if(t>7.8f && t<7.92f)input.Dodge=true;
                if(t>8 && t<9.6f)input.Move=Vector2.right*.6f;
                if(t>10.5f && t<10.62f)input.Move=Vector2.up;
                if(t>11 && t<15.3f)input.UltimateHeld=true;
                world.Input.InjectedFrame=input;
                if(t<next){yield return null;continue;}yield return new WaitForEndOfFrame();
                frames.Add(ScreenCapture.CaptureScreenshotAsTexture());timing.seconds.Add(Time.unscaledTime-born);
                Vector3 pixel=world.CameraRig.Camera.WorldToScreenPoint(mage.transform.position+Vector3.up*1.1f);timing.actorPixels.Add(new Vector2(pixel.x,Screen.height-pixel.y));next=t+1f/12;
            }
            world.Input.InjectedFrame=default(CombatInput);world.SetPaused(true);
            foreach(var frame in frames){File.WriteAllBytes(Path.Combine(output,"frame"+(index++).ToString("D4")+".png"),frame.EncodeToPNG());Destroy(frame);}
            File.WriteAllText(Path.Combine(output,"timing.json"),JsonUtility.ToJson(timing));
            Debug.Log("YUEYUE_PREVIEW_OK frames="+index);Application.Quit();
        }
    }
}

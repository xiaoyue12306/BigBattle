using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BigBattle
{
    // Actual normal-clock standalone recording. The fixtures supply souls before recording.
    public sealed class DoomPreview : MonoBehaviour
    {
        [Serializable]sealed class Timing {public List<float> seconds=new List<float>();}
        GameRoot world;
        public void Setup(GameRoot game){world=game;}
        IEnumerator Start()
        {
            while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;
            world.Input.Injected=true;world.SelectHero(HeroKind.Yueyue);yield return new WaitForSecondsRealtime(.2f);
            var mage=(YueyuePlayer)world.Player;mage.transform.position=new Vector3(-5,0,-5);
            Vector2 direction=new Vector2(.8f,.6f);world.Input.InjectedFrame=new CombatInput{Move=direction};yield return new WaitForSecondsRealtime(.15f);world.Input.InjectedFrame=default(CombatInput);
            for(int i=0;i<3;i++){var foe=world.SpawnEnemy(new Vector3(-9,0,5),1);foe.Damage(99,mage,Vector3.zero,0);}
            world.Dummy.transform.position=Vector3.left*9;
            for(int i=0;i<2;i++){var foe=world.SpawnEnemy(mage.transform.position+mage.Facing*(7+i*5),1000);foe.enabled=false;}
            world.CameraRig.CaptureDoom=true;world.CameraRig.Snap();
            world.Fx.Notice("万魂魔剑 · 竖斩 / 裂地 / 鬼焰风暴");
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Artifacts/DoomPreview"));Directory.CreateDirectory(output);
            var timing=new Timing();var frames=new List<Texture2D>();float born=Time.unscaledTime,next=0;int index=0;
            while(Time.unscaledTime-born<9.5f)
            {
                float t=Time.unscaledTime-born;world.Input.InjectedFrame=new CombatInput{UltimateHeld=t>.8f && t<5.15f};
                if(t<next){yield return null;continue;}yield return new WaitForEndOfFrame();
                frames.Add(ScreenCapture.CaptureScreenshotAsTexture());timing.seconds.Add(Time.unscaledTime-born);next=t+1f/18;
            }
            world.Input.InjectedFrame=default(CombatInput);world.SetPaused(true);
            foreach(var frame in frames){File.WriteAllBytes(Path.Combine(output,"frame"+(index++).ToString("D4")+".png"),frame.EncodeToPNG());Destroy(frame);}
            File.WriteAllText(Path.Combine(output,"timing.json"),JsonUtility.ToJson(timing));Debug.Log("DOOM_PREVIEW_OK frames="+index);Application.Quit();
        }
    }
}

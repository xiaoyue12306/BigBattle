using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BigBattle
{
    // Explicit capture mode only; no behaviour changes in ordinary play.
    public sealed class CombatPreview : MonoBehaviour
    {
        [Serializable] sealed class Timing {public List<float> seconds=new List<float>();}
        GameRoot world;
        public void Setup(GameRoot game){world=game;}
        IEnumerator Start()
        {
            while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;
            yield return new WaitForSecondsRealtime(.3f);
            world.Input.Injected=true;world.EnterPractice();world.Player.transform.position=new Vector3(0,0,-2.5f);
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Artifacts/MotionPreview"));Directory.CreateDirectory(output);
            var timing=new Timing();var frames=new List<Texture2D>();float born=Time.unscaledTime,nextCapture=0;int index=0;bool duel=false,clashed=false;
            while(Time.unscaledTime-born<4.8f)
            {
                float t=Time.unscaledTime-born;var input=new CombatInput();
                if(t<.3f)input.Move=Vector2.up;
                if(t>.42f && t<1.95f)input.Attack=true;
                if(t>=2.25f && !duel)
                {
                    duel=true;world.EnterSparring();world.Duelist.TrainingEnabled=false;
                    world.Player.transform.position=Vector3.back*.8f;world.Duelist.transform.position=Vector3.forward*.8f;
                }
                if(t>=2.28f && t<2.35f)input.Move=Vector2.up;
                if(t>=2.4f && !clashed)
                {
                    clashed=true;input.Attack=true;world.Duelist.Visual.PlayMotion(MotionId.Attack1,.34f);
                    world.Melee.Slash(world.Duelist,Vector3.back,0,.1f);
                }
                if(t>=3.3f && t<4.65f)input.Attack=true;
                world.Input.InjectedFrame=input;
                if(t<nextCapture){yield return null;continue;}
                yield return new WaitForEndOfFrame();
                Texture2D frame=ScreenCapture.CaptureScreenshotAsTexture();timing.seconds.Add(Time.unscaledTime-born);
                frames.Add(frame);nextCapture=t+.05f;
            }
            world.Input.InjectedFrame=default(CombatInput);world.SetPaused(true);
            // Encode after capture, avoiding PNG compression stalls in motion.
            foreach(var frame in frames){File.WriteAllBytes(Path.Combine(output,"frame"+(index++).ToString("D4")+".png"),frame.EncodeToPNG());Destroy(frame);}
            File.WriteAllText(Path.Combine(output,"timing.json"),JsonUtility.ToJson(timing));
            Debug.Log("COMBAT_PREVIEW_OK frames="+index);Application.Quit();
        }
    }
}

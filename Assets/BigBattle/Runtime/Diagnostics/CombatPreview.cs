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
            var timing=new Timing();var frames=new List<Texture2D>();float born=Time.unscaledTime,nextCapture=0,clashInputUntil=0;int index=0;bool duel=false,clashed=false;
            while(Time.unscaledTime-born<6.2f)
            {
                float t=Time.unscaledTime-born;var input=new CombatInput();
                if(t<.3f)input.Move=Vector2.up;
                if(t>.42f && t<1.55f)input.Attack=true;
                // Keep one-shot controls alive across an Update. An end-of-frame
                // capture can otherwise overwrite an injected edge before read.
                if(t>=1.80f && t<1.92f)input.Thrust=true;
                if(t>=2.30f && t<2.42f)input.Ultimate=true;
                if(t>=3.45f && !duel)
                {
                    duel=true;world.EnterSparring();world.Duelist.TrainingEnabled=false;
                    world.Player.transform.position=Vector3.back*.8f;world.Duelist.transform.position=Vector3.forward*.8f;
                }
                if(t>=3.48f && t<3.55f)input.Move=Vector2.up;
                if(t>=3.65f && !clashed)
                {
                    clashed=true;clashInputUntil=Time.unscaledTime+.12f;world.Duelist.Visual.PlayMotion(MotionId.Attack1,.34f);
                    world.Melee.Slash(world.Duelist,Vector3.back,0,.1f);
                }
                if(Time.unscaledTime<clashInputUntil)input.Attack=true;
                if(t>=4.30f && t<5.65f)input.Attack=true;
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

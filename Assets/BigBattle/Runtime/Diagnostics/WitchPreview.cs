using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BigBattle
{
    public sealed class WitchPreview : MonoBehaviour
    {
        [Serializable]sealed class Timing {public List<float> seconds=new List<float>();}
        GameRoot world;public void Setup(GameRoot game){world=game;}
        IEnumerator Start()
        {
            while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;
            world.Input.Injected=true;world.SelectHero(HeroKind.Witch);world.CameraRig.CaptureCloseup=true;world.CameraRig.CapturePortrait=true;world.CameraRig.CaptureSize=3.4f;world.CameraRig.Snap();
            // Warm the first model / material render before starting the measured capture.
            yield return new WaitForEndOfFrame();yield return null;
            world.Player.Visual.GetComponent<WitchVisual>().ResetPose();
            var frames=new List<Texture2D>();var timing=new Timing();float born=Time.unscaledTime,next=0;string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Artifacts/WitchPreview"));Directory.CreateDirectory(output);
            while(Time.unscaledTime-born<11.8f)
            {
                float t=Time.unscaledTime-born;var input=new CombatInput();
                if(t>2.6f && t<4.4f)input.Move=Vector2.up*.55f;
                if(t>4.5f && t<6.2f)input.Move=Vector2.right*.55f;
                if(t>6.3f && t<8.2f)input.Move=Vector2.down*.55f;
                if(t>8.3f && t<9.7f)input.Move=Vector2.left*.55f;
                if(t>9.8f && t<9.9f)input.Move=Vector2.down*.3f;
                world.Input.InjectedFrame=input;if(t<next){yield return null;continue;}yield return new WaitForEndOfFrame();frames.Add(ScreenCapture.CaptureScreenshotAsTexture());timing.seconds.Add(Time.unscaledTime-born);next=t+1f/12;
            }
            world.Input.InjectedFrame=default(CombatInput);world.SetPaused(true);int index=0;
            foreach(var frame in frames){File.WriteAllBytes(Path.Combine(output,"frame"+(index++).ToString("D4")+".png"),frame.EncodeToPNG());Destroy(frame);}
            File.WriteAllText(Path.Combine(output,"timing.json"),JsonUtility.ToJson(timing));Debug.Log("WITCH_PREVIEW_OK frames="+index);Application.Quit();
        }
    }
}

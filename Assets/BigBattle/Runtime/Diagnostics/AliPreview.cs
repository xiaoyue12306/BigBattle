using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BigBattle
{
    public sealed class AliPreview : MonoBehaviour
    {
        [Serializable]sealed class Timing {public List<float> seconds=new List<float>();}
        GameRoot world;public void Setup(GameRoot game){world=game;}
        IEnumerator Start()
        {
            while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;
            world.Input.Injected=true;world.SelectHero(HeroKind.Ali);world.Player.transform.position=new Vector3(-3,0,-2);
            world.CameraRig.CaptureCloseup=true;world.CameraRig.CaptureSize=7;world.CameraRig.Snap();
            yield return new WaitForEndOfFrame();yield return null;world.Player.Visual.GetComponent<AliVisual>().ResetPose();
            var frames=new List<Texture2D>();var timing=new Timing();float born=Time.unscaledTime,next=0;string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Artifacts/AliPreview"));Directory.CreateDirectory(output);
            while(Time.unscaledTime-born<11.8f)
            {
                float t=Time.unscaledTime-born;var input=new CombatInput();
                if(t>1 && t<2.12f){input.Move=Vector2.right;input.Attack=true;}
                if(t>4.8f && t<5.9f){input.Move=Vector2.up;input.Attack=true;}
                if(t>8.6f && t<9.75f){input.Move=Vector2.left;input.Attack=true;}
                world.Input.InjectedFrame=input;if(t<next){yield return null;continue;}yield return new WaitForEndOfFrame();frames.Add(ScreenCapture.CaptureScreenshotAsTexture());timing.seconds.Add(Time.unscaledTime-born);next=t+1f/15;
            }
            world.Input.InjectedFrame=default(CombatInput);world.SetPaused(true);int index=0;
            foreach(var frame in frames){File.WriteAllBytes(Path.Combine(output,"frame"+(index++).ToString("D4")+".png"),frame.EncodeToPNG());Destroy(frame);}
            File.WriteAllText(Path.Combine(output,"timing.json"),JsonUtility.ToJson(timing));Debug.Log("ALI_PREVIEW_OK frames="+index);Application.Quit();
        }
    }
}

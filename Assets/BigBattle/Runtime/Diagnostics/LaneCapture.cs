using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BigBattle
{
    [DefaultExecutionOrder(-100)]
    public sealed class LaneCapture : MonoBehaviour
    {
        [Serializable]sealed class Timing {public List<float> seconds=new List<float>();}
        [Serializable]sealed class Run {public string hero,result;public float seconds,enemyCore,alliedCore;public int waves,alliedKills,enemyKills;}
        [Serializable]sealed class Report {public string utc=DateTime.UtcNow.ToString("o");public int maxGameSeconds=900;public List<Run> runs=new List<Run>();public List<string> errors=new List<string>();}
        GameRoot world;bool simulation;LaneHeroBot brain;Report report=new Report();
        public void Setup(GameRoot game,bool fast){world=game;simulation=fast;}
        void Update(){if(brain!=null && world.Input!=null)world.Input.InjectedFrame=brain.Plan();}
        void Log(string text,string stack,LogType type){if(type==LogType.Error || type==LogType.Exception)report.errors.Add(text+"\n"+stack);}
        IEnumerator Start()
        {
            if(!simulation)while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;
            Application.logMessageReceived+=Log;world.Input.Injected=true;
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,simulation?"../../../Artifacts/LaneSim":"../../../Artifacts/LanePreview"));Directory.CreateDirectory(output);
            if(simulation)
            {
                Time.captureFramerate=10;QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;
                world.CameraRig.Camera.enabled=false;world.GetComponent<GameHud>().enabled=false;
                foreach(HeroKind kind in new[]{HeroKind.Banana,HeroKind.Yueyue,HeroKind.Ali})
                {
                    world.SelectHero(kind);world.StartLaneBattle();brain=world.Player.gameObject.AddComponent<LaneHeroBot>();brain.ExternalInput=true;brain.Setup(world.Lane,world.Player,0);
                    float born=Time.time;
                    while(world.State==SessionState.Battle && Time.time-born<report.maxGameSeconds)yield return null;
                    var mode=world.Lane;
                    report.runs.Add(new Run{hero=world.Player.HeroName,result=world.State.ToString(),seconds=Time.time-born,enemyCore=mode.Cores[1]!=null?mode.Cores[1].Health:0,alliedCore=mode.Cores[0]!=null?mode.Cores[0].Health:0,waves=mode.WavesSpawned,alliedKills=mode.HeroKills[0],enemyKills=mode.HeroKills[1]});
                    brain=null;world.EnterPractice();yield return null;
                }
                Time.captureFramerate=0;File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));
                bool pass=report.errors.Count==0 && report.runs.TrueForAll(r=>r.result=="Victory" || r.result=="Defeat");Debug.Log("LANE_SIM_"+(pass?"PASS":"FAIL"));Application.Quit(pass?0:1);yield break;
            }
            world.SelectHero(HeroKind.Yueyue);world.StartLaneBattle();world.CameraRig.MapOverview=true;world.CameraRig.Snap();yield return new WaitForSecondsRealtime(.3f);
            yield return new WaitForEndOfFrame();var overview=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,"map-overview.png"),overview.EncodeToPNG());Destroy(overview);
            world.CameraRig.MapOverview=false;world.CameraRig.Snap();brain=world.Player.gameObject.AddComponent<LaneHeroBot>();brain.ExternalInput=true;brain.Setup(world.Lane,world.Player,0);
            var timing=new Timing();var frames=new List<Texture2D>();float began=Time.unscaledTime,next=0;int index=0;
            while(Time.unscaledTime-began<18)
            {
                float now=Time.unscaledTime-began;if(now<next){yield return null;continue;}
                yield return new WaitForEndOfFrame();frames.Add(ScreenCapture.CaptureScreenshotAsTexture());timing.seconds.Add(Time.unscaledTime-began);next=now+1f/12;
            }
            world.SetPaused(true);foreach(var frame in frames){File.WriteAllBytes(Path.Combine(output,"frame"+(index++).ToString("D4")+".png"),frame.EncodeToPNG());Destroy(frame);}
            File.WriteAllText(Path.Combine(output,"timing.json"),JsonUtility.ToJson(timing));Debug.Log("LANE_PREVIEW_OK frames="+index);Application.Quit();
        }
        void OnDestroy(){Application.logMessageReceived-=Log;Time.captureFramerate=0;}
    }
}

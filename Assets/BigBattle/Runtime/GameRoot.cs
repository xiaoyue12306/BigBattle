using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    public enum SessionState { Practice,Battle,Victory,Defeat }

    public sealed class GameRoot : MonoBehaviour
    {
        public readonly List<Combatant> Actors=new List<Combatant>();
        public BananaPlayer Player {get;private set;}
        public Combatant Dummy {get;private set;}
        public LocalInput Input {get;private set;}
        public ArenaCamera CameraRig {get;private set;}
        public Feedback Fx {get;private set;}
        public GameSound Sound {get;private set;}
        public Transform Transients {get;private set;}
        public SessionState State {get;private set;}
        public bool Paused {get;private set;}
        public bool SmokeTesting {get;private set;}
        public int Wave {get;private set;}
        public int Score {get;set;}
        public int MenuSelection {get;private set;}
        public int EnemyCount {get {int count=0;foreach(var actor in Actors)if(actor!=null && actor.Alive && !actor.IsDummy && actor.Team==1)count++;return count;}}
        public float BattleSeconds {get;private set;}
        public int HitPauseCount {get;private set;}
        public bool HitPausing=>!Paused && Time.unscaledTime<hitPauseUntil;
        float hitPauseUntil,nextHitPause;
        float nextWaveAt;

        void Awake()
        {
            Application.targetFrameRate=60;Application.runInBackground=true;
            QualitySettings.antiAliasing=4;QualitySettings.shadowDistance=65;
            Cursor.visible=false;Cursor.lockState=CursorLockMode.None;
            bool smoke=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--smoke-test")>=0;
            bool preview=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--preview-capture")>=0;
            SmokeTesting=smoke || preview;
            var garden=new GameObject("Original garden arena").AddComponent<GardenArena>();garden.transform.SetParent(transform);garden.Build();
            Transients=new GameObject("Session objects").transform;Transients.SetParent(transform);
            Fx=new GameObject("Feedback").AddComponent<Feedback>();Fx.transform.SetParent(transform);
            Sound=gameObject.AddComponent<GameSound>();Sound.Setup();
            Input=new LocalInput(new InputBinding {Kind=InputKind.Keyboard,DeviceIndex=0,KeyboardLayout=0},true);
            Player=new GameObject("Banana assassin • Player 1").AddComponent<BananaPlayer>();Player.transform.SetParent(transform);Player.Setup(this);
            CameraRig=new GameObject("Player 1 camera").AddComponent<ArenaCamera>();CameraRig.transform.SetParent(transform);CameraRig.Setup(this);
            gameObject.AddComponent<GameHud>().Setup(this);
            EnterPractice();
            if(smoke)gameObject.AddComponent<SmokeTest>().Setup(this);
            else if(preview)gameObject.AddComponent<CombatPreview>().Setup(this);
        }

        void Update()
        {
            ApplyClock();
            var input=Input.Read();
            if(input.Pause){SetPaused(!Paused);return;}
            if(Paused)
            {
                if(input.MenuStep!=0)MenuSelection=(MenuSelection+input.MenuStep+5)%5;
                if(input.Confirm)ActivateMenu();return;
            }
            if(State==SessionState.Victory || State==SessionState.Defeat)
            {
                if(input.Confirm || input.Session)StartBattle();return;
            }
            if(input.Session){if(State==SessionState.Practice)StartBattle();else EnterPractice();return;}
            Player.Tick(input);
            if(State==SessionState.Battle)
            {
                BattleSeconds+=Time.deltaTime;
                if(EnemyCount==0)
                {
                    if(Wave>=3)Finish(true);
                    else if(nextWaveAt==0){nextWaveAt=Time.time+2;Fx.Notice("本波完成 · 下一波即将到来");}
                    else if(Time.time>=nextWaveAt)SpawnWave(Wave+1);
                }
            }
        }

        public void SetPaused(bool paused)
        {
            Paused=paused;ApplyClock();MenuSelection=0;
            if(paused)Input.StopRumble();
        }
        void ApplyClock(){Time.timeScale=Paused?0:HitPausing?.08f:1;}
        public void HitPause(bool heavy)
        {
            if(Paused || Time.unscaledTime<nextHitPause)return;
            hitPauseUntil=Time.unscaledTime+(heavy?.044f:.028f);nextHitPause=Time.unscaledTime+.075f;
            HitPauseCount++;ApplyClock();
        }
        void ActivateMenu()
        {
            switch(MenuSelection)
            {
                case 0:SetPaused(false);break;
                case 1:SetPaused(false);StartBattle();break;
                case 2:SetPaused(false);EnterPractice();break;
                case 3:Sound.Muted=!Sound.Muted;break;
                case 4:
                    Input.StopRumble();
                    #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying=false;
                    #else
                    Application.Quit();
                    #endif
                    break;
            }
        }
        void ClearSession()
        {
            hitPauseUntil=nextHitPause=0;
            SetPaused(false);
            for(int i=Actors.Count-1;i>=0;i--)if(Actors[i]!=Player && Actors[i]!=null)Destroy(Actors[i].gameObject);
            Actors.RemoveAll(actor=>actor!=Player);Dummy=null;
            foreach(Transform child in Transients)Destroy(child.gameObject);
            Fx.Clear();Player.ResetCombat();Player.transform.position=new Vector3(0,0,-4);
            Wave=0;Score=0;BattleSeconds=0;nextWaveAt=0;CameraRig.Snap();
        }
        public void EnterPractice()
        {
            ClearSession();State=SessionState.Practice;
            Dummy=new GameObject("Training dummy").AddComponent<Combatant>();Dummy.transform.SetParent(transform);Dummy.transform.position=new Vector3(0,0,1);
            Dummy.transform.rotation=Quaternion.Euler(0,180,0);Dummy.Initialize(this,1,240,2,true);
            Fx.Notice("欢迎来到香蕉花园 · 靠近木桩试试身手");
        }
        public void StartBattle(){ClearSession();State=SessionState.Battle;SpawnWave(1);}
        public void SpawnWave(int wave)
        {
            Wave=wave;nextWaveAt=0;
            int count=wave+1;
            for(int i=0;i<count;i++)
            {
                float a=(i-(count-1)*.5f)*.65f;
                Vector3 point=new Vector3(Mathf.Sin(a)*7,0,Mathf.Cos(a)*7);
                SpawnEnemy(point,48+wave*10);
            }
            Fx.Notice("第 "+wave+" / 3 波 · 小心团子的红色预警圈！");
        }
        public MochiEnemy SpawnEnemy(Vector3 point,float health=60)
        {
            var enemy=new GameObject("Garden mochi").AddComponent<MochiEnemy>();enemy.transform.SetParent(transform);enemy.transform.position=GardenArena.Clamp(point);enemy.Setup(this,health);return enemy;
        }
        public void Finish(bool victory)
        {
            if(State==SessionState.Victory || State==SessionState.Defeat)return;
            State=victory?SessionState.Victory:SessionState.Defeat;Input.StopRumble();
            Player.Visual.Animate(0,false);
            if(victory)Player.Visual.PlayMotion(MotionId.Victory,1);
            if(victory){Sound.Play(SoundCue.Win);Fx.Burst(Player.transform.position+Vector3.up,Art.Yellow,18);}
            foreach(var actor in Actors)if(actor!=null && actor!=Player)actor.enabled=false;
        }
        public Vector3 Assist(Combatant source,Vector3 facing,float range)
        {
            Combatant best=null;float bestDistance=range;
            foreach(var actor in Actors)
            {
                if(actor==null || actor.Team==source.Team || !actor.Alive)continue;
                Vector3 offset=actor.transform.position-source.transform.position;offset.y=0;
                float distance=offset.magnitude;
                if(distance<bestDistance && distance>.05f && Vector3.Dot(facing,offset.normalized)>.93f){best=actor;bestDistance=distance;}
            }
            if(best==null)return facing;
            return Vector3.RotateTowards(facing,(best.transform.position-source.transform.position).normalized,.18f,0).normalized;
        }
        public int HitArea(Combatant source,Vector3 center,float range,Vector3 direction,float minimumDot,float damage,float push)
        {
            int hits=0;
            // Damage can remove actors; iterate a snapshot to avoid skipping enemies.
            Combatant[] targets=Actors.ToArray();
            foreach(var actor in targets)
            {
                if(actor==null || !actor.Alive || actor.Team==source.Team)continue;
                Vector3 offset=actor.transform.position-center;offset.y=0;
                if(offset.magnitude>range+actor.Radius)continue;
                if(minimumDot>-1 && offset.sqrMagnitude>.01f && Vector3.Dot(direction,offset.normalized)<minimumDot)continue;
                Vector3 away=(actor.transform.position-source.transform.position).normalized;
                if(actor.Damage(damage,source,away*push))hits++;
            }
            return hits;
        }
        public PeelTrap SpawnPeel(Vector3 origin,Vector3 direction)
        {
            var peel=new GameObject("Banana peel trap").AddComponent<PeelTrap>();peel.transform.SetParent(Transients);peel.Setup(this,Player,origin,direction);return peel;
        }
        public int HitCapsule(Combatant source,Vector3 from,Vector3 to,float radius,float damage,float push)
        {
            int hits=0;Vector3 segment=to-from;float length=segment.sqrMagnitude;
            foreach(var actor in Actors.ToArray())
            {
                if(actor==null || !actor.Alive || actor.Team==source.Team)continue;
                float t=length<.001f?0:Mathf.Clamp01(Vector3.Dot(actor.transform.position-from,segment)/length);
                Vector3 closest=from+segment*t;
                if((actor.transform.position-closest).sqrMagnitude>(radius+actor.Radius)*(radius+actor.Radius))continue;
                if(actor.Damage(damage,source,(actor.transform.position-from).normalized*push))hits++;
            }
            return hits;
        }
        void OnApplicationFocus(bool focus){if(!focus && !SmokeTesting && Input!=null && !Paused)SetPaused(true);}
        void OnApplicationQuit(){if(Input!=null)Input.StopRumble();Time.timeScale=1;}
        void OnDestroy(){Time.timeScale=1;if(Input!=null)Input.StopRumble();Art.Cleanup();}
    }
}

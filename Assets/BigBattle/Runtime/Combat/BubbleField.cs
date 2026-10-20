using System.Collections.Generic;
using UnityEngine;

namespace BigBattle
{
    // Central bounded simulation: mark/remove a bomb BEFORE emitting its wave.
    // The expanding footprint sweeps every traversed point even on a long frame.
    public sealed class BubbleField : MonoBehaviour
    {
        public const int OwnerLimit=5,GlobalLimit=96;
        public const float Fuse=1.8f,Length=5,HalfWidth=.6f,TravelTime=.15f,WaveLifetime=.58f;
        public sealed class Bomb
        {
            public Combatant Owner;public Vector3 Position,Across,Forward;public float Born,Damage;public BubbleVisual Visual;
        }
        public sealed class Wave
        {
            public Combatant Owner;public Vector3 Position,Across,Forward;public float Damage,Age,Reach;public float[] Limits;
            public BubbleVisual Visual;public readonly HashSet<Combatant> Hit=new HashSet<Combatant>();
        }
        readonly List<Bomb> bombs=new List<Bomb>();readonly List<Wave> waves=new List<Wave>();
        GameRoot world;
        public int PendingCount=>bombs.Count;
        public int WaveCount=>waves.Count;
        public int Detonations {get;private set;}
        public int ChainDetonations {get;private set;}
        public IReadOnlyList<Bomb> Bombs=>bombs;
        public IReadOnlyList<Wave> Waves=>waves;
        public void Setup(GameRoot game){world=game;}
        public int PendingFor(Combatant owner){int n=0;foreach(var bomb in bombs)if(bomb.Owner==owner)n++;return n;}
        public bool Place(Combatant owner,Vector3 position,float damage)
        {
            if(owner==null || !owner.Alive || world.Paused || PendingFor(owner)>=OwnerLimit || bombs.Count+waves.Count>=GlobalLimit)return false;
            var bomb=new Bomb{Owner=owner,Position=world.Clamp(position),Across=world.IsLane?FlowerLaneMap.Across:Vector3.right,Forward=world.IsLane?FlowerLaneMap.Forward(0):Vector3.forward,Born=Time.time,Damage=damage};
            bomb.Visual=BubbleVisual.Bomb(transform,bomb.Position);bombs.Add(bomb);world.Sound.Play(SoundCue.BubblePlace,.8f);return true;
        }
        float FloorLength(Vector3 start,Vector3 direction)
        {
            float length=0;for(int i=1;i<=50;i++){Vector3 point=start+direction*(i*.1f);if((world.Clamp(point)-point).sqrMagnitude>.0004f)break;length=i*.1f;}return length;
        }
        void Detonate(Bomb bomb,bool chain)
        {
            if(!bombs.Remove(bomb))return;
            if(bomb.Visual!=null)Destroy(bomb.Visual.gameObject);
            var wave=new Wave{Owner=bomb.Owner,Position=bomb.Position,Across=bomb.Across,Forward=bomb.Forward,Damage=bomb.Damage};
            wave.Limits=new[]{FloorLength(wave.Position,wave.Across),FloorLength(wave.Position,-wave.Across),FloorLength(wave.Position,wave.Forward),FloorLength(wave.Position,-wave.Forward)};
            wave.Visual=BubbleVisual.Blast(transform,wave.Position,wave.Across,wave.Forward,wave.Limits);waves.Add(wave);Detonations++;if(chain)ChainDetonations++;
            world.Sound.Play(SoundCue.BubbleBurst,.85f);if(bomb.Owner==world.Player)world.CameraRig.Shake(.045f);
        }
        // A rectangular cross, not a radial blast. Radius expands the target only.
        public static bool InCross(Wave w,Vector3 point,float radius)
        {
            Vector3 d=point-w.Position;float x=Vector3.Dot(d,w.Across),z=Vector3.Dot(d,w.Forward),width=HalfWidth+radius;
            return Mathf.Abs(z)<=width && x<=Mathf.Min(w.Reach,w.Limits[0])+radius && x>=-Mathf.Min(w.Reach,w.Limits[1])-radius
                || Mathf.Abs(x)<=width && z<=Mathf.Min(w.Reach,w.Limits[2])+radius && z>=-Mathf.Min(w.Reach,w.Limits[3])-radius;
        }
        void Update()
        {
            if(world.Paused)return;
            if(world.State!=SessionState.Practice && world.State!=SessionState.Battle){Clear();return;}
            foreach(var bomb in bombs.ToArray())
            {
                if(bomb.Owner==null){bombs.Remove(bomb);if(bomb.Visual!=null)Destroy(bomb.Visual.gameObject);continue;}
                float age=Time.time-bomb.Born;if(age>=Fuse)Detonate(bomb,false);else bomb.Visual.PaintBomb(age);
            }
            // Iterative chain resolution has at most GlobalLimit iterations;
            // no recursion and no duplicate explosion, even in a closed loop.
            for(int i=0;i<waves.Count;i++)
            {
                var wave=waves[i];float previousAge=wave.Age;wave.Age+=Time.deltaTime;wave.Reach=Mathf.Min(Length,wave.Age/TravelTime*Length);
                if(wave.Owner!=null && previousAge<=TravelTime)
                {
                    foreach(var bomb in bombs.ToArray())if(InCross(wave,bomb.Position,.4f))Detonate(bomb,true);
                    foreach(var actor in world.Actors.ToArray())
                    {
                        if(actor==null || actor.Team==wave.Owner.Team || !world.Targetable(actor) || wave.Hit.Contains(actor) || !InCross(wave,actor.transform.position,actor.Radius))continue;
                        wave.Hit.Add(actor);actor.Damage(wave.Damage,wave.Owner,Vector3.zero,0);
                        if(world.State!=SessionState.Practice && world.State!=SessionState.Battle){Clear();return;}
                    }
                }
                wave.Visual.PaintBlast(wave.Age,wave.Reach);
            }
            for(int i=waves.Count-1;i>=0;i--)if(waves[i].Age>=WaveLifetime){Destroy(waves[i].Visual.gameObject);waves.RemoveAt(i);}
        }
        public void Clear()
        {
            foreach(var bomb in bombs)if(bomb.Visual!=null)Destroy(bomb.Visual.gameObject);
            foreach(var wave in waves)if(wave.Visual!=null)Destroy(wave.Visual.gameObject);
            bombs.Clear();waves.Clear();
        }
    }
}

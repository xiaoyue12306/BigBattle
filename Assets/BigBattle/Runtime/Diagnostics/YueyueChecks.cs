using System.Collections;
using UnityEngine;

namespace BigBattle
{
    public sealed partial class SmokeTest
    {
        IEnumerator YueyueChecks()
        {
            Frame();world.OpenSelection();Check(world.Selecting,"Hero selection opens without mouse");yield return Shot("12-hero-selection");
            world.SelectHero(HeroKind.Yueyue);yield return Wait(.15f);
            var mage=world.Player as YueyuePlayer;
            Check(mage!=null && mage.HeroName=="越越大王" && mage.MaxHealth==155,"Selecting Yueyue creates the independent hero and HUD action set");
            var rig=mage.Visual.GetComponent<YueyueVisual>();var skin=mage.Visual.GetComponentInChildren<SkinnedMeshRenderer>();
            bool textures=true;foreach(string name in SoulVfx.TextureNames)textures&=Resources.Load<Texture2D>("SoulVfx/"+name)!=null;
            var soulShader=Shader.Find("BigBattle/SoulParticle");Check(textures && soulShader!=null && soulShader.isSupported,"Thirteen licensed online particle textures and native soul shader are present in the Windows player");
            var bloom=Shader.Find("Hidden/BigBattle/SoulBloom");Check(bloom!=null && bloom.isSupported && world.CameraRig.Camera.allowHDR && world.CameraRig.GetComponent<SoulBloom>()!=null,"Selective HDR soul glow shader and native camera compositor survive player stripping");
            Check(rig!=null && rig.HasModel && rig.BoneCount>=10 && skin!=null && skin.bones.Length>=13 && skin.sharedMaterial.mainTexture!=null,"User FBX imports with authored bones, skin and texture");
            bool sheets=true;foreach(string name in SoulVfx.SheetNames){var texture=Resources.Load<Texture2D>("SoulVfx/Flipbooks/"+name);sheets&=texture!=null && texture.width>256;}
            Check(sheets,"Seven new licensed animation atlases survive import at usable resolution");
            yield return MageMotionChecks(mage,rig,skin);
            mage.transform.position=Vector3.back*3;Frame(new CombatInput{Move=Vector2.up});yield return Wait(.04f);Frame();
            float before=world.Dummy.TotalDamageReceived;
            Frame(new CombatInput{Attack=true});yield return Wait(.12f);Frame();
            var skull=world.Transients.GetComponentInChildren<GhostProjectile>();
            Check(skull!=null && !skull.Flying && world.Dummy.TotalDamageReceived==before,"Skull waits near its caster before launch");
            Check(skull!=null && skull.GetComponentInChildren<SoulVfx>()!=null && skull.GetComponentsInChildren<ParticleSystem>().Length>=2,"Skull carries textured ghost flame, smoke and spark emitters");
            yield return Shot("13-yueyue-skull");yield return Wait(.75f);
            Check(world.Dummy.TotalDamageReceived>=before+28,"Accelerating homing skull hits the forward target");
            float casts=mage.Casts(0);Frame(new CombatInput{Attack=true});yield return Wait(1.4f);Frame();
            Check(mage.Casts(0)>=casts+2,"Holding basic attack repeatedly launches skulls");
            world.EnterPractice();yield return Wait(.08f);mage.transform.position=Vector3.back*3;
            Frame(new CombatInput{Move=Vector2.up});yield return Wait(.04f);Frame();
            var enemy=world.SpawnEnemy(Vector3.zero,200);enemy.enabled=false;
            Check(mage.Cast(1,Vector3.zero),"Iron chain starts a real forward projectile");yield return Wait(.28f);
            Check(enemy.BoundUntil>Time.time+1.5f && enemy.Controlled,"Chain contact applies a two-second movement and action bind");
            Check(mage.Remaining(1)>9 && !mage.Cast(1,Vector3.zero),"Chain cooldown begins at cast and rejects repeats");
            Check(world.Transients.GetComponentInChildren<SoulCage>()!=null,"Void chains surround the bound victim");
            var cage=world.Transients.GetComponentInChildren<SoulCage>();Check(cage!=null && cage.GetComponentInChildren<SoulVfx>()!=null,"Actual chain bind displays layered void portals and its ground seal");
            yield return Shot("14-void-chain-bind");
            Vector3 frozen=enemy.transform.position;world.SetPaused(true);float left=enemy.BoundUntil-Time.time;yield return Wait(.15f);world.SetPaused(false);
            Check(Mathf.Abs(enemy.BoundUntil-Time.time-left)<.04f,"Pause freezes remaining chain duration");
            yield return Wait(2.1f);Check(!enemy.Controlled && enemy.transform.position==frozen,"Bind expires without residual movement");
            // A killable enemy is deliberately different from an immortal practice dummy.
            world.EnterPractice();yield return Wait(.08f);Check(!mage.Cast(2,Vector3.zero),"Summon is unavailable before the hero owns a defeated enemy");
            enemy=world.SpawnEnemy(Vector3.forward*3,80);enemy.enabled=false;enemy.Damage(999,mage,Vector3.zero,0);
            Check(mage.Souls==1 && mage.Fallen.Valid && mage.Fallen.Style==1,"Personal enemy defeat records its appearance and adds one soul");
            world.Dummy.Damage(999,mage,Vector3.zero,0);Check(mage.Souls==1,"Immortal dummy resets do not award souls");
            Check(mage.Cast(2,Vector3.zero),"Recorded enemy can be summoned");yield return Wait(.08f);var ally=mage.Servant;
            Check(ally!=null && ally.Team==mage.Team && ally.MaxHealth==40 && Mathf.Abs(ally.BaseAttackDamage-6.5f)<.001f,"Returned ally has exactly half the recorded health and attack");
            Check(ally.GetComponentInChildren<SpiritAura>()!=null && mage.Remaining(2)>24,"Summon has a teal aura, one slot and a 25-second cooldown");
            before=world.Dummy.TotalDamageReceived;ally.transform.position=world.Dummy.transform.position+Vector3.back;yield return Wait(.95f);
            Check(world.Dummy.TotalDamageReceived>before,"Returned enemy autonomously attacks an opposing target");yield return Shot("15-returned-spirit");
            var victim=world.SpawnEnemy(ally.transform.position+Vector3.forward*.7f,1);victim.enabled=false;world.Dummy.transform.position=Vector3.right*8;
            yield return Wait(1.5f);Check(mage.Souls==2,"Summon's actual autonomous kill awards its owner a soul");
            Check(!mage.Cast(4,Vector3.zero),"Ultimate rejects fewer than three earned souls");
            var attacker=world.SpawnEnemy(Vector3.right*6,100);attacker.enabled=false;
            mage.Bind(2);Check(mage.Controlled && mage.Cast(3,Vector3.zero) && !mage.Controlled,"Ghost form cleanses an existing bind");
            float hp=mage.Health;Check(!mage.Damage(10,attacker,Vector3.zero,2) && !mage.Bind(2) && mage.Health==hp,"Ghost form rejects both damage and control");
            Check(!mage.Cast(0,Vector3.zero) && !mage.Cast(1,Vector3.zero) && !mage.Cast(2,Vector3.zero) && !mage.Cast(4,Vector3.zero),"Ghost form disables offensive actions and summoning");
            Vector3 pos=mage.transform.position;Frame(new CombatInput{Move=Vector2.right});yield return Wait(.25f);Frame();
            Check(Vector3.Distance(mage.transform.position,pos)>.5f && mage.Remaining(3)>7,"Ghost form retains movement and starts its eight-second cooldown");yield return Shot("16-void-ghost");
            var phaseFx=System.Array.Find(mage.GetComponentsInChildren<SoulVfx>(),f=>f.Kind==SoulEffect.Phase);Check(phaseFx!=null && phaseFx.EmitterCount>=2,"Ghost form displays a live textured soul wake rather than only a transparent skin");
            yield return Wait(2.8f);Check(!mage.Phasing && mage.Damage(1,attacker,Vector3.zero,0),"Ghost invulnerability ends after three seconds");
            mage.FillPracticeSouls();Frame(new CombatInput{UltimateHeld=true});yield return Wait(.25f);Frame();yield return Wait(.05f);
            Check(!mage.Charging && mage.Souls==3 && mage.Remaining(4)==0,"Early charge release cancels without spending souls or starting cooldown");
            Frame(new CombatInput{UltimateHeld=true});yield return Wait(.20f);mage.Bind(2);yield return Wait(.03f);
            Check(!mage.Charging && mage.Souls==3,"Hard control interrupts sword charging without consuming souls");
            yield return Wait(2.1f);Check(!mage.Charging,"Held key cannot restart an interrupted charge until released");Frame();yield return Wait(.04f);
            world.Dummy.transform.position=Vector3.right*8;mage.transform.position=new Vector3(0,0,-4);
            // Let the actual 20-second lifetime end before creating exact-damage fixtures.
            if(ally!=null)yield return Wait(Mathf.Max(0,ally.ExpiresAt-Time.time)+.15f);
            Check(ally==null,"Returned spirit retires after its twenty-second lifetime");
            var a=world.SpawnEnemy(Vector3.zero,1000);a.enabled=false;var b=world.SpawnEnemy(Vector3.forward*5,1000);b.enabled=false;
            var distant=world.SpawnEnemy(Vector3.forward*6,1000);distant.enabled=false;distant.transform.position=Vector3.forward*30;
            pos=mage.transform.position;Frame(new CombatInput{Move=Vector2.up,UltimateHeld=true});yield return Wait(.5f);
            float travelled=Vector3.Distance(pos,mage.transform.position);Check(mage.Charging && travelled>.10f && travelled<.40f,"Charging sword reduces movement by ninety percent");
            mage.Damage(1,attacker,Vector3.zero);Check(mage.Charging,"Ordinary damage does not interrupt the four-second charge");
            Frame(new CombatInput{UltimateHeld=true});yield return Wait(3.7f);
            Check(mage.Charging && mage.ChargeProgress>=1 && mage.ReleasedWaves==0,"Full four-second charge waits for key release");yield return Shot("17-soul-sword-charge");
            Check(Vector3.Distance(rig.SwordPosition,rig.HandPosition(true))<.35f && Vector3.Distance(rig.SwordPosition,rig.HandPosition(false))<.35f && rig.SwordPosition.y>mage.transform.position.y+1.7f,"Raised sword follows both actual skinned hand joints above the shoulders");
            bool unstretched=true;foreach(var bone in skin.bones)unstretched&=Vector3.Distance(bone.localScale,Vector3.one)<.001f;Check(unstretched,"Two-arm sword grip uses joint rotations without stretching limb scales");
            world.SetPaused(true);float charge=mage.ChargeProgress;yield return Wait(.12f);Check(mage.ChargeProgress==charge,"Pause freezes sword charging");world.SetPaused(false);
            var chargeFx=System.Array.Find(mage.GetComponentsInChildren<SoulVfx>(),f=>f.Kind==SoulEffect.Charge);
            var chargeEmitter=chargeFx!=null?chargeFx.GetComponentInChildren<ParticleSystem>():null;
            Check(chargeEmitter!=null && chargeEmitter.particleCount>0,"Full soul charge emits textured gathering sparks and ghost fire");
            float atlasBefore=chargeFx.AnimationFrame;yield return Wait(.12f);Check(chargeFx.AnimatedLayers>=3 && chargeFx.AnimationFrame!=atlasBefore,"Real charge advances multiple imported flame and arcane flipbooks");
            world.SetPaused(true);float atlasPaused=chargeFx.AnimationFrame;float particleClock=chargeEmitter!=null?chargeEmitter.time:0;Quaternion rune=chargeFx!=null?chargeFx.transform.GetChild(0).localRotation:Quaternion.identity;yield return Wait(.12f);
            float particleDelta=chargeEmitter!=null?Mathf.Abs(chargeEmitter.time-particleClock):-1;float sealDelta=Quaternion.Angle(rune,chargeFx.transform.GetChild(0).localRotation);
            Check(chargeEmitter!=null && particleDelta<.001f && rune==chargeFx.transform.GetChild(0).localRotation,"Pause freezes imported particles and charge seals (particle="+particleDelta.ToString("0.000000")+", seal="+sealDelta.ToString("0.000000")+")");world.SetPaused(false);
            Check(atlasPaused==chargeFx.AnimationFrame,"Pause freezes the animated atlas sampling clock as well");
            Frame();yield return Wait(.12f);yield return Shot("18-doom-sword-wave");
            var waveVisual=world.Transients.GetComponentInChildren<DoomWaveVisual>();var upright=waveVisual!=null?waveVisual.GetComponentInChildren<MeshFilter>():null;
            Check(upright!=null && upright.GetComponent<Renderer>().bounds.size.y>8 && upright.sharedMesh.bounds.size.y>upright.sharedMesh.bounds.size.x*5,"Doom is a towering world-up volume rather than a horizontal slash card");
            Check(Shader.Find("BigBattle/DoomEnergy")!=null && Shader.Find("BigBattle/DoomEnergy").isSupported,"Dedicated black-core HDR doom shader survives Windows stripping");
            yield return Wait(.6f);
            Check(mage.ReleasedWaves==1 && mage.Souls==0 && mage.Remaining(4)>19,"Releasing full charge spends three souls and starts twenty-second cooldown");
            Check(a.Health==500 && b.Health==500,"Sword wave pierces two enemies for exactly five hundred damage each");
            Check(!mage.Cast(4,Vector3.zero) && world.Fx.transform.Find("Doom fractured ground")!=null,"Ultimate rejects immediate repeat and leaves temporary ground fissures");
            yield return Shot("19-doom-ground-cracks");
            yield return Wait(.9f);Check(distant.Health==500 && a.Health==500 && b.Health==500,"Doom travels beyond the arena and never damages the same target twice");
            for(int i=0;i<3;i++){var sacrifice=world.SpawnEnemy(Vector3.right*8,1);sacrifice.enabled=false;sacrifice.Damage(10,mage,Vector3.zero,0);}
            Check(mage.Souls==3 && mage.Remaining(4)>17 && !mage.Cast(4,Vector3.zero),"Twenty-second cooldown blocks ultimate even after earning three new souls");
            world.EnterPractice();yield return Wait(.12f);Check(mage.Souls==0 && !mage.Fallen.Valid && !mage.Charging && mage.Servant==null && world.Transients.childCount==0,"Session reset clears souls, record, ally, projectiles and charge");
            mage.FillPracticeSouls();Frame(new CombatInput{UltimateHeld=true});yield return Wait(.1f);world.EnterPractice();Frame();yield return Wait(.08f);
            Check(!mage.Charging && !rig.SwordVisible,"Reset during active charge removes the raised sword and soul currents");
            Check(System.Array.Find(mage.GetComponentsInChildren<SoulVfx>(),f=>f.Kind==SoulEffect.Charge || f.Kind==SoulEffect.Phase)==null,"Reset deactivates reusable charge and phase emitters without residual particles");
            GhostProjectile.Spawn(world,mage,GhostShot.Doom,Vector3.forward);world.EnterPractice();yield return Wait(.1f);
            Check(world.Dummy.TotalDamageReceived==0 && world.Transients.GetComponentInChildren<GhostProjectile>()==null,"Old projectiles cannot damage new-session actors during deferred destruction");
            mage.transform.position=Vector3.back*4;
            var bananaFoe=new GameObject("Killable enemy banana fixture").AddComponent<BananaDuelist>();bananaFoe.transform.SetParent(world.transform);bananaFoe.SetupEnemy(world);bananaFoe.TrainingEnabled=false;bananaFoe.Damage(999,mage,Vector3.zero,0);
            Check(mage.Fallen.Style==0 && mage.Cast(2,Vector3.zero) && mage.Servant.MaxHealth==60,"Defeated enemy banana returns with its own model and half health");
            mage.Servant.transform.position=world.Dummy.transform.position-Vector3.forward*1.3f;before=world.Dummy.TotalDamageReceived;yield return Wait(1.0f);
            Check(world.Dummy.TotalDamageReceived>=before+12 && world.Dummy.TotalDamageReceived<before+24,"Returned banana uses a real half-strength twelve-damage melee attack");
            world.EnterPractice();yield return Wait(.08f);
            var mageFoe=new GameObject("Killable enemy mage fixture").AddComponent<YueyueDuelist>();mageFoe.transform.SetParent(world.transform);mageFoe.SetupEnemy(world);mageFoe.TrainingEnabled=false;mageFoe.Damage(999,mage,Vector3.zero,0);
            Check(mage.Fallen.Style==3 && mage.Cast(2,Vector3.zero) && mage.Servant.MaxHealth==110,"Defeated enemy Yueyue returns with its supplied model and half health");
            mage.Servant.transform.position=Vector3.back*2;before=world.Dummy.TotalDamageReceived;yield return Wait(1.45f);
            Check(world.Dummy.TotalDamageReceived>=before+14 && world.Dummy.TotalDamageReceived<before+28,"Returned Yueyue uses a real half-strength fourteen-damage skull");
            world.EnterPractice();yield return Wait(.08f);mage.FillPracticeSouls();Frame(new CombatInput{UltimateHeld=true});yield return Wait(.1f);
            attacker=world.SpawnEnemy(Vector3.right*8,100);attacker.enabled=false;mage.Damage(999,attacker,Vector3.zero,0);Frame();
            Check(!mage.Charging && mage.ReleasedWaves==1 && world.State==SessionState.Defeat,"Death interrupts charge and never releases a stale sword wave");
            world.EnterPractice();yield return Wait(.08f);
            yield return DoomVisualChecks(mage);
            SoulVfx.Burst(world,SoulEffect.Impact,Vector3.up);yield return Wait(.7f);int materials=Art.SharedMaterialCount;
            for(int i=0;i<20;i++)SoulVfx.Burst(world,SoulEffect.Impact,new Vector3((i%5-2)*.3f,1,i/5*.3f));yield return Wait(.7f);
            Check(world.Fx.GetComponentInChildren<SoulVfx>()==null && Art.SharedMaterialCount==materials,"Repeated online-texture impacts retire fully and reuse warmed shared materials");
            world.SetOpponent(HeroKind.Yueyue);world.EnterSparring();yield return Wait(.15f);
            Check(world.PracticeFoe is YueyueDuelist,"Practice opponent supports Yueyue as well as banana");yield return Shot("20-yueyue-opponent");
            world.SetOpponent(HeroKind.Banana);world.SelectHero(HeroKind.Banana);yield return Wait(.12f);
            Check(!(world.Player is YueyuePlayer) && world.Player.HeroName=="香蕉刺客" && world.Player.Health==180,"Switching back preserves original banana combat setup");
        }
        IEnumerator DoomVisualChecks(YueyuePlayer mage)
        {
            Mesh shared=null;bool vertical=true,reused=true;int warmMaterials=Art.SharedMaterialCount;
            foreach(Vector3 heading in new[]{Vector3.forward,Vector3.right,Vector3.back,new Vector3(-1,0,1).normalized})
            {
                var shot=GhostProjectile.Spawn(world,mage,GhostShot.Doom,heading);var volume=shot.GetComponentInChildren<DoomWaveVisual>().GetComponentInChildren<MeshFilter>();
                Bounds bounds=volume.GetComponent<Renderer>().bounds;vertical&=bounds.size.y>8 && bounds.size.y>Mathf.Max(bounds.size.x,bounds.size.z)*1.8f;
                if(shared!=null)reused&=volume.sharedMesh==shared;shared=volume.sharedMesh;
            }
            yield return Wait(.25f);Check(vertical && reused,"Forward, side, reverse and diagonal sword waves stay vertical and reuse the same 3D mesh");
            var rifts=world.Fx.GetComponentsInChildren<DoomRift>();bool continuous=rifts.Length==4;
            foreach(var rift in rifts)for(int j=1;j<rift.transform.childCount;j++)continuous&=Vector3.Distance(rift.transform.GetChild(j-1).localPosition,rift.transform.GetChild(j).localPosition)<1.3f;
            Check(continuous && DoomRift.LiveSegments>8 && DoomRift.LiveSegments<=DoomRift.SegmentLimit,"Distance-based overlapping fractures remain continuous and bounded for simultaneous waves");
            var surface=rifts[0].GetComponentInChildren<Renderer>();var beforeBlock=new MaterialPropertyBlock();surface.GetPropertyBlock(beforeBlock);float clock=beforeBlock.GetFloat("_Clock");int marks=DoomRift.LiveSegments;
            world.SetPaused(true);yield return Wait(.15f);var afterBlock=new MaterialPropertyBlock();surface.GetPropertyBlock(afterBlock);
            Check(DoomRift.LiveSegments==marks && afterBlock.GetFloat("_Clock")==clock,"Pause freezes travelling rifts and their animated emissive shader clock");world.SetPaused(false);
            yield return Wait(7.8f);Check(DoomRift.LiveSegments==0 && world.Fx.GetComponentInChildren<DoomRift>()==null,"Every fissure, shard and flame expires after the wave without permanent terrain objects");
            Check(Art.SharedMaterialCount==warmMaterials,"Repeated towering waves, scorches and fractures reuse warmed materials without permanent growth");
            GhostProjectile.Spawn(world,mage,GhostShot.Doom,Vector3.forward);yield return Wait(.08f);world.EnterPractice();yield return Wait(.08f);
            Check(DoomRift.LiveSegments==0 && world.Fx.GetComponentInChildren<DoomRift>()==null,"Retry clears the live fracture budget and all doom ground marks");
        }
        IEnumerator MageMotionChecks(YueyuePlayer mage,YueyueVisual rig,SkinnedMeshRenderer skin)
        {
            mage.transform.position=new Vector3(-4,0,-4);Frame(new CombatInput{Move=Vector2.right});yield return Wait(.4f);
            var hip=System.Array.Find(skin.bones,b=>b.name=="Pelvis");var arm=System.Array.Find(skin.bones,b=>b.name=="RightUpperArm");
            Quaternion hipA=hip.localRotation,armA=arm.localRotation;float stride=rig.StridePhase;yield return Wait(.15f);
            Check(rig.BoneCount==13 && rig.WalkWeight>.6f && stride!=rig.StridePhase && Quaternion.Angle(hipA,hip.localRotation)>1 && Quaternion.Angle(armA,arm.localRotation)>2,"Actual movement drives the pelvis, opposite arm swing and distance-based stride");
            world.SetPaused(true);stride=rig.StridePhase;hipA=hip.localRotation;armA=arm.localRotation;yield return Wait(.12f);
            Check(rig.StridePhase==stride && hip.localRotation==hipA && arm.localRotation==armA,"Pause freezes the full caster skeleton and locomotion phase");world.SetPaused(false);Frame();yield return Wait(.4f);
            stride=rig.StridePhase;yield return Wait(.12f);Check(rig.WalkWeight<.05f && rig.StridePhase==stride,"Stopping blends to idle without continuing a sliding walk cycle");
            var seen=new System.Collections.Generic.HashSet<int>();
            for(int i=0;i<3;i++){Check(mage.Cast(0,Vector3.zero),"Cosmetic casting variant starts through normal attack "+i);yield return Wait(.08f);seen.Add(rig.BasicVariant);yield return Wait(.65f);}
            Check(seen.Count==3,"Three consecutive skull attacks use distinct authored casting gestures");
            world.EnterPractice();yield return Wait(.08f);Check(rig.WalkWeight==0 && rig.CastingSkill==-1 && !rig.SwordVisible,"Session reset removes locomotion momentum and active casting poses");
        }
    }
}

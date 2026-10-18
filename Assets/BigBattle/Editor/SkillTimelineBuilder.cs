using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace BigBattle.Editor
{
    // Assets are created only when absent. Building never overwrites an artist's Timeline edits.
    public static class SkillTimelineBuilder
    {
        const string Root="Assets/BigBattle/Resources/SkillTimelines/";
        static readonly Color Teal=new Color(.03f,.95f,.65f),Blue=new Color(.08f,.5f,1),Hot=new Color(.6f,1,.85f),Red=new Color(.9f,.07f,.19f),Gold=new Color(1,.66f,.08f);
        [MenuItem("Tools/BigBattle/4. Create missing skill Timelines")]
        public static void EnsureAssets()
        {
            Directory.CreateDirectory(Root);
            AssetDatabase.Refresh();
            foreach(string name in SkillSequence.Names)
            {
                if(AssetDatabase.LoadAssetAtPath<TimelineAsset>(Root+name+".playable")!=null)continue;
                var asset=ScriptableObject.CreateInstance<TimelineAsset>();asset.name=name;AssetDatabase.CreateAsset(asset,Root+name+".playable");Build(asset,name);EditorUtility.SetDirty(asset);
            }
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        }
        static SkillVfxClip Add(TimelineAsset asset,string label,SequenceShape shape,float start,float length,float from,float to,Color tint,Vector3 offset,float opacity=1,bool ground=true,float spin=0)
        {
            var track=asset.CreateTrack<SkillVfxTrack>(null,label);var clip=track.CreateClip<SkillVfxClip>();clip.displayName=label;clip.start=start;clip.duration=length;
            var data=(SkillVfxClip)clip.asset;data.Shape=shape;data.Tint=tint;data.From=Vector3.one*from;data.To=Vector3.one*to;data.Offset=offset;data.Opacity=opacity;data.Ground=ground;data.Spin=spin;EditorUtility.SetDirty(data);return data;
        }
        static void Hold(SkillVfxClip clip,float begin=.05f,float end=1){clip.Envelope=new AnimationCurve(new Keyframe(0,begin),new Keyframe(.35f,.55f),new Keyframe(1,end));EditorUtility.SetDirty(clip);}
        static SkillVfxClip Ray(TimelineAsset asset,string label,float start,float life,float size,Vector3 offset,Color tint,float alpha)
        {
            var data=Add(asset,label,SequenceShape.Rays,start,life,size*.55f,size,tint,offset,alpha,false);data.Billboard=true;return data;
        }
        static void Build(TimelineAsset a,string name)
        {
            Vector3 floor=Vector3.up*.045f;
            if(name=="SoulCharge")
            {
                Hold(Add(a,"00 · 聚能曝光曲线",SequenceShape.Envelope,0,4,1,1,Teal,Vector3.zero),.30f,1.1f);
                Hold(Add(a,"01 · 符文逐层显现",SequenceShape.Seal,0,4,3.2f,7,Teal,floor,.5f,true,-20),.05f,.9f);
                Add(a,"02 · 第一束收缩涟漪",SequenceShape.Ring,.2f,1.0f,7,2.5f,Blue,floor+Vector3.up*.02f,.55f);
                Add(a,"03 · 第二束收缩涟漪",SequenceShape.Ring,1.35f,1.1f,8,3,Teal,floor+Vector3.up*.04f,.65f);
                var spiral=Add(a,"04 · 双螺旋魂风升起",SequenceShape.Spiral,.55f,3.45f,1,1,Teal,floor,.60f,false,100);spiral.From=new Vector3(2,.72f,2);spiral.To=new Vector3(1.4f,1,1.4f);Hold(spiral);
                Hold(Add(a,"05 · 魂晶悬浮加速",SequenceShape.Shards,1,3,1.4f,2.8f,Blue,Vector3.up*.7f,.65f,false,-45));
                var tip=Ray(a,"06 · 剑尖魂核",.7f,3.3f,3,Vector3.zero,Hot,.45f);tip.FollowSword=true;Hold(tip,.05f,.75f);
                Hold(Add(a,"07 · 蓄满红色外环",SequenceShape.Arc,3,1,3.1f,3.5f,Red,Vector3.up*.14f,.42f,false,-65),.05f,.75f);
            }
            if(name=="SoulRelease")
            {
                floor=Vector3.down*.455f;
                Ray(a,"01 · 瞬间高光",0,.17f,8,Vector3.up*1.3f,Hot,.85f);
                Add(a,"02 · 内层冲击",SequenceShape.Ring,.02f,.48f,1,10,Teal,floor,.85f);
                Add(a,"03 · 延迟外层冲击",SequenceShape.Ring,.12f,.66f,2,16,Blue,floor+Vector3.up*.018f,.65f);
                Add(a,"04 · 破碎法阵余波",SequenceShape.Seal,.04f,1.05f,5,9,Teal,floor+Vector3.up*.035f,.52f,true,-38);
                var jet=Add(a,"05 · 向上喷薄魂柱",SequenceShape.Spiral,.02f,.75f,1,1,Blue,Vector3.down*.4f,.70f,false,160);jet.From=new Vector3(.6f,.3f,.6f);jet.To=new Vector3(2.4f,1.8f,2.4f);
                Add(a,"06 · 魂晶爆散",SequenceShape.Shards,.03f,.85f,.8f,5,Teal,Vector3.up*.4f,.85f,false,25);
                Add(a,"07 · 红色末端余波",SequenceShape.Ring,.2f,.65f,3,11,Red,floor+Vector3.up*.05f,.32f);
                var envelope=Add(a,"00 · 爆发转为余波",SequenceShape.Envelope,0,1.1f,1,1,Teal,Vector3.zero);envelope.Envelope=new AnimationCurve(new Keyframe(0,1.1f),new Keyframe(.2f,.95f),new Keyframe(1,.35f));
            }
            if(name=="SoulCage")
            {
                Add(a,"01 · 锁定冲击",SequenceShape.Ring,0,.35f,.5f,3.4f,Blue,floor,.8f);
                var seal=Add(a,"02 · 禁锢符文",SequenceShape.Seal,0,2,3.1f,3.4f,Teal,floor+Vector3.up*.015f,.65f,true,25);seal.Envelope=new AnimationCurve(new Keyframe(0,0),new Keyframe(.08f,1),new Keyframe(.9f,.7f),new Keyframe(1,0));
                Add(a,"03 · 魂晶环绕",SequenceShape.Shards,.05f,1.95f,1.3f,1.5f,Blue,Vector3.up*.8f,.7f,false,35);
            }
            if(name=="SoulSummon")
            {
                Add(a,"01 · 召回门扉",SequenceShape.Seal,0,1.1f,1.2f,4.8f,Teal,floor,.7f,true,-35);
                Add(a,"02 · 外层波纹",SequenceShape.Ring,.08f,.65f,1,5.2f,Blue,floor+Vector3.up*.02f,.6f);
                var s=Add(a,"03 · 螺旋塑形",SequenceShape.Spiral,.08f,.9f,1,1,Teal,floor,.7f,false,150);s.From=new Vector3(.4f,.2f,.4f);s.To=new Vector3(1.3f,1,1.3f);
                Add(a,"04 · 归魂碎晶",SequenceShape.Shards,.15f,.95f,.6f,2.6f,Hot,Vector3.up*.4f,.65f,false,-60);
                Ray(a,"05 · 成形高光",.25f,.3f,3,Vector3.up,Hot,.6f);
            }
            if(name=="SoulPhase")
            {
                Add(a,"01 · 鬼影入场涟漪",SequenceShape.Ring,0,.5f,.4f,3.5f,Blue,floor,.6f);
                Add(a,"02 · 虚无魂晶",SequenceShape.Shards,0,3,1.1f,1.4f,Teal,Vector3.up*.4f,.45f,false,80);
                Add(a,"03 · 三秒鬼气轮廓",SequenceShape.Seal,0,3,2.2f,2.5f,Blue,floor,.35f,true,-45);
            }
            if(name=="SoulImpact")
            {
                Ray(a,"01 · 命中亮芯",0,.16f,2.5f,Vector3.zero,Hot,.85f);
                var ring=Add(a,"02 · 魂冲涟漪",SequenceShape.Ring,.02f,.3f,.3f,2.2f,Teal,Vector3.zero,.6f,false);ring.Billboard=true;
                Add(a,"03 · 飞散碎魂",SequenceShape.Shards,.02f,.42f,.25f,1.5f,Blue,Vector3.zero,.65f,false,60);
            }
            if(name=="SoulSkull")
            {
                var ring=Add(a,"01 · 悬停聚魂",SequenceShape.Seal,0,.36f,.5f,1.15f,Teal,Vector3.zero,.5f,false,80);ring.Billboard=true;
                Ray(a,"02 · 加速出膛",.33f,.22f,1.6f,Vector3.zero,Hot,.75f);
                Add(a,"03 · 随行魂屑",SequenceShape.Shards,.34f,3.15f,.4f,.7f,Blue,Vector3.zero,.6f,false,130);
            }
            if(name=="BananaSlash" || name=="BananaFinisher")
            {
                bool final=name=="BananaFinisher";float life=final?.42f:.25f;Color color=final?Gold:Teal;
                var arc=Add(a,"01 · 接触刃缘回响",SequenceShape.Arc,0,life,final?1.6f:1.3f,final?2.8f:2, color,Vector3.up*.65f,.55f,false,80);arc.Tilt=new Vector3(14,-35,0);
                var second=Add(a,"02 · 反向流光",SequenceShape.Arc,.025f,life*.8f,1.1f,2.1f,final?Teal:Gold,Vector3.up*.8f,.4f,false,-110);second.Tilt=new Vector3(-18,25,0);
                Add(a,"03 · 叶形星屑散开",SequenceShape.Shards,.02f,life,.6f,final?2.8f:1.9f,color,Vector3.up*.55f,.65f,false,35);
                if(final){Add(a,"04 · 重击地面环",SequenceShape.Ring,.025f,.4f,.5f,4.5f,Gold,floor,.65f);Ray(a,"05 · 重击星芒",0,.18f,2.2f,Vector3.forward*1.5f+Vector3.up*.75f,Hot,.7f);}
            }
            if(name=="SteelClash")
            {
                Ray(a,"01 · 碰撞金白亮芯",0,.12f,3,Vector3.zero,Hot,.9f);
                var r=Add(a,"02 · 空气冲击圈",SequenceShape.Ring,.015f,.26f,.2f,2.8f,Gold,Vector3.zero,.65f,false);r.Billboard=true;
                Add(a,"03 · 金属火星",SequenceShape.Shards,0,.3f,.25f,2.3f,Gold,Vector3.zero,.75f,false,120);
            }
        }
        [MenuItem("Tools/BigBattle/5. Create release Timeline preview")]
        public static void Preview()
        {
            EnsureAssets();var go=new GameObject("Skill Timeline preview • select Director then open Timeline");Undo.RegisterCreatedObjectUndo(go,"Create skill preview");go.AddComponent<SkillSequence>().Setup(null,AssetDatabase.LoadAssetAtPath<TimelineAsset>(Root+"SoulRelease.playable"));Selection.activeGameObject=go;
        }
        [MenuItem("Tools/BigBattle/6. Validate Timeline authoring preview")]
        public static void ValidatePreview()
        {
            int clips=0;foreach(string name in SkillSequence.Names)
            {
                var asset=AssetDatabase.LoadAssetAtPath<TimelineAsset>(Root+name+".playable");if(asset==null)throw new System.Exception("Missing skill Timeline "+name);
                foreach(var track in asset.GetOutputTracks())foreach(var clip in track.GetClips()){if(!(clip.asset is SkillVfxClip) || clip.duration<=0)throw new System.Exception("Invalid visual clip");clips++;}
            }
            var go=new GameObject("Temporary Timeline authoring validation");
            int errors=0;Application.LogCallback capture=(message,stack,type)=>{if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert)errors++;};Application.logMessageReceived+=capture;
            try
            {
                var sequence=go.AddComponent<SkillSequence>();sequence.Setup(null,AssetDatabase.LoadAssetAtPath<TimelineAsset>(Root+"SoulRelease.playable"));
                sequence.Director.time=.07;sequence.Director.Evaluate();int visible=0;foreach(var r in go.GetComponentsInChildren<Renderer>())if(r.enabled)visible++;
                if(visible<3)throw new System.Exception("Timeline editor preview failed to draw staged layers");
                var flash=go.transform.Find("01 · 瞬间高光").GetComponent<Renderer>();sequence.Director.time=.9;sequence.Director.Evaluate();if(flash.enabled)throw new System.Exception("Inactive clip remains after seeking forward");
                sequence.Director.time=.07;sequence.Director.Evaluate();if(!flash.enabled)throw new System.Exception("Timeline backwards scrub fails to restore the flash");
            }
            finally {Object.DestroyImmediate(go);Application.logMessageReceived-=capture;}
            if(errors>0)throw new System.Exception("Timeline authoring emitted "+errors+" errors");
            Debug.Log("BIGBATTLE_TIMELINES_OK assets="+SkillSequence.Names.Length+" clips="+clips+" editorScrub=PASS");
        }
    }
}

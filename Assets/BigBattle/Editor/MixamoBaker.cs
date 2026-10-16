using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace BigBattle.Editor
{
    public static class MixamoBaker
    {
        const string Output="Assets/BigBattle/Resources/LocalMixamo/Library.asset";
        static readonly string[] Names={"idle","walk","run","slash","slash (3)","attack (2)","attack (4)","casting (2)","crouching (3)","slash (4)","impact (2)","death","power up"};
        [MenuItem("Tools/BigBattle/3. Bake local Mixamo motions")]
        public static void Bake()
        {
            if(!Directory.Exists("Assets/LocalMixamoSource")){Debug.Log("MIXAMO_NOT_PRESENT: using original animation fallback.");return;}
            AssetDatabase.Refresh();
            var tracks=new List<MotionTrack>();
            float referenceHeight=0;
            for(int index=0;index<Names.Length;index++)
            {
                string path="Assets/LocalMixamoSource/sword and shield "+Names[index]+".fbx";
                var importer=AssetImporter.GetAtPath(path) as ModelImporter;
                if(importer==null)throw new Exception("Missing local Mixamo source: "+path);
                if(importer.animationType!=ModelImporterAnimationType.Generic || importer.animationCompression!=ModelImporterAnimationCompression.Off)
                {
                    importer.animationType=ModelImporterAnimationType.Generic;importer.importAnimation=true;
                    importer.animationCompression=ModelImporterAnimationCompression.Off;importer.materialImportMode=ModelImporterMaterialImportMode.None;
                    importer.SaveAndReimport();
                }
                AnimationClip clip=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c=>!c.name.StartsWith("__"));
                if(clip==null || clip.length<.01f)throw new Exception("No animation in "+path);
                GameObject skeleton=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                skeleton.hideFlags=HideFlags.HideAndDontSave;
                try
                {
                    Transform[] bones=skeleton.GetComponentsInChildren<Transform>();
                    Func<string,Transform> bone=name=>bones.FirstOrDefault(t=>t.name.Replace("mixamorig:","").Replace("mixamorig","")==name);
                    Transform hips=bone("Hips"),spine=bone("Spine2")??bone("Spine"),head=bone("Head"),lf=bone("LeftFoot"),rf=bone("RightFoot");
                    Transform lh=bone("LeftHand"),rh=bone("RightHand"),la=bone("LeftForeArm"),ra=bone("RightForeArm");
                    if(new[]{hips,spine,head,lf,rf,lh,rh,la,ra}.Any(t=>t==null))throw new Exception("Incomplete Mixamo skeleton in "+path+": "+string.Join(",",bones.Select(t=>t.name)));
                    Transform root=skeleton.transform;
                    Func<Transform,Vector3> local=t=>root.InverseTransformPoint(t.position);
                    Vector3 restHip=local(hips),restLF=local(lf),restRF=local(rf);
                    if(referenceHeight==0)referenceHeight=Mathf.Max(.01f,local(head).y-Mathf.Min(restLF.y,restRF.y));
                    float height=referenceHeight;
                    Quaternion restSpine=Quaternion.Inverse(root.rotation)*spine.rotation;
                    int samples=Mathf.Max(2,Mathf.CeilToInt(clip.length*60)+1);
                    var track=new MotionTrack {Id=(MotionId)index,Source=Path.GetFileName(path),Duration=clip.length,Loop=index<3,Frames=new MotionPose[samples]};
                    for(int frame=0;frame<samples;frame++)
                    {
                        clip.SampleAnimation(skeleton,clip.length*frame/(samples-1));
                        Vector3 hip=local(hips);
                        Vector3 delta=(Quaternion.Inverse(root.rotation)*spine.rotation*Quaternion.Inverse(restSpine)).eulerAngles;
                        delta=new Vector3(Signed(delta.x),Signed(delta.y),Signed(delta.z));
                        track.Frames[frame]=new MotionPose {
                            Height=Mathf.Clamp((hip.y-restHip.y)/height*.65f,-.23f,.16f),
                            Torso=new Vector3(Mathf.Clamp(delta.x,-22,24),Mathf.Clamp(delta.y,-40,40),Mathf.Clamp(delta.z,-16,16)),
                            LeftHand=Hand(local(lh)-hip,height,-1),RightHand=Hand(local(rh)-hip,height,1),
                            LeftFoot=Foot(local(lf)-restLF-new Vector3(hip.x-restHip.x,0,hip.z-restHip.z),height,-1),
                            RightFoot=Foot(local(rf)-restRF-new Vector3(hip.x-restHip.x,0,hip.z-restHip.z),height,1),
                            LeftGrip=Quaternion.Slerp(Quaternion.identity,Quaternion.FromToRotation(Vector3.up,(local(lh)-local(la)).normalized),.55f),
                            RightGrip=Quaternion.Slerp(Quaternion.identity,Quaternion.FromToRotation(Vector3.up,(local(rh)-local(ra)).normalized),.55f)};
                    }
                    float maximum=0;
                    for(int frame=samples/5;frame<samples*4/5;frame++)
                    {
                        float speed=(track.Frames[frame].RightHand-track.Frames[frame-1].RightHand).sqrMagnitude;
                        if(speed>maximum){maximum=speed;track.Contact=frame/(float)(samples-1);}
                    }
                    tracks.Add(track);
                    Debug.Log("MIXAMO_BAKED "+track.Id+" source="+track.Source+" seconds="+clip.length+" samples="+samples+" contact="+track.Contact+" height="+height);
                }
                finally {Object.DestroyImmediate(skeleton);}
            }
            Directory.CreateDirectory(Path.GetDirectoryName(Output));AssetDatabase.Refresh();
            var library=AssetDatabase.LoadAssetAtPath<MixamoMotionLibrary>(Output);
            if(library==null){library=ScriptableObject.CreateInstance<MixamoMotionLibrary>();AssetDatabase.CreateAsset(library,Output);}
            library.Tracks=tracks.ToArray();EditorUtility.SetDirty(library);AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Artifacts");File.WriteAllText("Artifacts/mixamo-bake.txt",string.Join("\n",tracks.Select(t=>t.Id+" <- "+t.Source+" ("+t.Frames.Length+" samples)")));
            Debug.Log("MIXAMO_LIBRARY_OK "+tracks.Count+" tracks");
        }
        static float Signed(float degrees)=>Mathf.DeltaAngle(0,degrees);
        static Vector3 Hand(Vector3 offset,float height,int side)
        {
            float x=Mathf.Clamp(Mathf.Abs(offset.x/height*1.65f),.37f,.78f)*side;
            return new Vector3(x,Mathf.Clamp(1.08f+offset.y/height*1.35f,.67f,1.65f),Mathf.Clamp(.05f+offset.z/height*1.7f,-.35f,.85f));
        }
        static Vector3 Foot(Vector3 offset,float height,int side)
        {
            return new Vector3(side*.23f+Mathf.Clamp(offset.x/height*.65f,-.10f,.10f),.20f+Mathf.Clamp(offset.y/height*1.3f,0,.30f),.06f+Mathf.Clamp(offset.z/height*1.3f,-.38f,.38f));
        }
    }
}

using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BigBattle.Editor
{
    public static class ProjectBuilder
    {
        public const string ScenePath="Assets/BigBattle/Scenes/BananaGarden.unity";
        [MenuItem("Tools/BigBattle/1. Generate and open garden scene")]
        public static void CreateScene()
        {
            Directory.CreateDirectory("Assets/BigBattle/Scenes");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("BigBattle • Game session").AddComponent<GameRoot>();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),ScenePath);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            AssetDatabase.SaveAssets();
        }
        [MenuItem("Tools/BigBattle/2. Build Windows x64")]
        public static void BuildWindows()
        {
            Configure();
            if(!File.Exists(ScenePath))CreateScene();
            Directory.CreateDirectory("Builds/Windows");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{ScenePath},locationPathName="Builds/Windows/BigBattle.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            Directory.CreateDirectory("Artifacts");
            File.WriteAllText("Artifacts/build-result.txt",report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings+"\nBytes: "+report.summary.totalSize);
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Windows build failed: "+report.summary.result);
            Debug.Log("BIGBATTLE_BUILD_OK "+report.summary.totalSize+" bytes");
        }
        public static void Build(){MixamoBaker.Bake();CreateScene();BuildWindows();}
        static void Configure()
        {
            PlayerSettings.companyName="BigBattle";PlayerSettings.productName="Banana Brawl";PlayerSettings.bundleVersion="0.8.0";
            PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;
            PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
            PlayerSettings.colorSpace=ColorSpace.Linear;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone,ScriptingImplementation.Mono2x);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone,ApiCompatibilityLevel.NET_Standard_2_0);
            PlayerSettings.stripEngineCode=false;PlayerSettings.usePlayerLog=true;
            // A newly added AssetPostprocessor can compile after a texture's
            // first import. Ensure the same final settings on that first build.
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/BigBattle/Resources/SoulVfx"}))
            {
                var importer=AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureImporter;
                int size=AssetDatabase.GUIDToAssetPath(guid).Contains("/Flipbooks/")?2048:512;
                if(importer!=null && importer.maxTextureSize!=size){importer.maxTextureSize=size;importer.SaveAndReimport();}
            }
            // All art is generated at runtime, so retain the shaders explicitly.
            var graphicsAssets=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
            if(graphicsAssets.Length>0)
            {
                var graphics=new SerializedObject(graphicsAssets[0]);
                var included=graphics.FindProperty("m_AlwaysIncludedShaders");
                // IMGUI's internal text/colored shaders are supplied by the
                // engine. Adding their editor-only references causes DontSave
                // serialization failures in Tuanjie player builds.
                for(int i=included.arraySize-1;i>=0;i--)
                {
                    var shader=included.GetArrayElementAtIndex(i).objectReferenceValue as Shader;
                    if(shader!=null && (shader.name=="GUI/Text Shader" || shader.name=="Hidden/Internal-Colored"))
                    {
                        included.GetArrayElementAtIndex(i).objectReferenceValue=null;
                        included.DeleteArrayElementAtIndex(i);
                    }
                }
                foreach(string shaderName in new[]{"Standard","Unlit/Color","BigBattle/CombatFX","BigBattle/BladeEnergy","BigBattle/SoulParticle","Hidden/BigBattle/SoulBloom"})
                {
                    Shader shader=Shader.Find(shaderName);if(shader==null)continue;
                    bool found=false;for(int i=0;i<included.arraySize;i++)if(included.GetArrayElementAtIndex(i).objectReferenceValue==shader)found=true;
                    if(!found){int index=included.arraySize;included.InsertArrayElementAtIndex(index);included.GetArrayElementAtIndex(index).objectReferenceValue=shader;}
                }
                graphics.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSettings.serializationMode=SerializationMode.ForceText;QualitySettings.vSyncCount=1;
            QualitySettings.antiAliasing=4;QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;QualitySettings.shadowDistance=65;
            AssetDatabase.SaveAssets();
        }
    }
}

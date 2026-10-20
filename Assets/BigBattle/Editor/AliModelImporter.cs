using UnityEditor;
using UnityEngine;

namespace BigBattle.Editor
{
    public sealed class AliModelImporter : AssetPostprocessor
    {
        const string Path="Assets/BigBattle/Resources/Ali/Ali.fbx";
        static void Apply(ModelImporter i){i.animationType=ModelImporterAnimationType.Legacy;i.importAnimation=true;i.animationCompression=ModelImporterAnimationCompression.Off;i.importCameras=false;i.importLights=false;i.materialImportMode=ModelImporterMaterialImportMode.None;}
        void OnPreprocessModel(){if(assetPath==Path)Apply((ModelImporter)assetImporter);}
        public static void Ensure()
        {
            var importer=AssetImporter.GetAtPath(Path) as ModelImporter;if(importer==null)throw new System.Exception("Missing Ali FBX");
            if(importer.animationType!=ModelImporterAnimationType.Legacy || !importer.importAnimation || importer.materialImportMode!=ModelImporterMaterialImportMode.None){Apply(importer);importer.SaveAndReimport();}
            int count=0;foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(Path))if(asset is AnimationClip && !asset.name.StartsWith("__preview__"))count++;
            if(count!=4)throw new System.Exception("Ali requires four clips, found "+count);Debug.Log("ALI_IMPORT_OK clips="+count);
        }
    }
}

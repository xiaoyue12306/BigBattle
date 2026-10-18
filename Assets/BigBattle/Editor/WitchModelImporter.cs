using UnityEditor;
using UnityEngine;

namespace BigBattle.Editor
{
    public sealed class WitchModelImporter : AssetPostprocessor
    {
        const string Path="Assets/BigBattle/Resources/Witch/Witch.fbx";
        void OnPreprocessModel(){if(assetPath==Path)Apply((ModelImporter)assetImporter);}
        static void Apply(ModelImporter importer)
        {
            importer.animationType=ModelImporterAnimationType.Legacy;importer.importAnimation=true;importer.animationCompression=ModelImporterAnimationCompression.Off;
            importer.importCameras=false;importer.importLights=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;
        }
        public static void Ensure()
        {
            var importer=AssetImporter.GetAtPath(Path) as ModelImporter;if(importer==null)throw new System.Exception("Missing prepared Witch FBX");
            if(importer.animationType!=ModelImporterAnimationType.Legacy || !importer.importAnimation || importer.materialImportMode!=ModelImporterMaterialImportMode.None){Apply(importer);importer.SaveAndReimport();}
            int count=0;foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(Path))if(asset is AnimationClip && !asset.name.StartsWith("__preview__"))count++;
            if(count<3)throw new System.Exception("Witch requires three authored FBX animation clips, found "+count);
            Debug.Log("WITCH_IMPORT_OK clips="+count);
        }
    }
}

using UnityEngine;

namespace BigBattle
{
    public sealed class GardenArena : MonoBehaviour
    {
        public const float WalkRadius = 10.6f;
        public static Vector3 Clamp(Vector3 position) { position.y = 0; return Vector3.ClampMagnitude(position, WalkRadius); }

        public void Build()
        {
            var sky = new Color(.63f,.84f,.89f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.65f,.74f,.82f);
            RenderSettings.ambientEquatorColor = new Color(.52f,.65f,.59f);
            RenderSettings.ambientGroundColor = new Color(.28f,.38f,.32f);
            RenderSettings.fog = true; RenderSettings.fogColor = sky; RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 42; RenderSettings.fogEndDistance = 85;
            var sun = new GameObject("Warm soft sunlight").AddComponent<Light>(); sun.transform.SetParent(transform); sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(48,-32,0); sun.intensity = .85f; sun.color = new Color(1,.95f,.84f); sun.shadows = LightShadows.Soft; sun.shadowStrength = .30f; sun.shadowBias = .035f;
            Art.Shape("Pastel water", PrimitiveType.Cube, transform, new Vector3(0,-1.15f,0), new Vector3(180,.20f,180), new Color(.43f,.74f,.81f));
            Art.Shape("Island foundation", PrimitiveType.Cylinder, transform, new Vector3(0,-.72f,0), new Vector3(27.6f,.50f,27.6f), new Color(.74f,.77f,.59f));
            Art.Shape("Ivory island rim", PrimitiveType.Cylinder, transform, new Vector3(0,-.34f,0), new Vector3(27.9f,.19f,27.9f), new Color(.98f,.90f,.69f));
            Art.Shape("Garden lawn", PrimitiveType.Cylinder, transform, new Vector3(0,-.18f,0), new Vector3(27.3f,.13f,27.3f), new Color(.54f,.76f,.46f));
            Art.Shape("Arena sand", PrimitiveType.Cylinder, transform, new Vector3(0,-.035f,0), new Vector3(22.4f,.035f,22.4f), new Color(.94f,.87f,.65f));
            Art.Ring("Arena mint boundary",transform,10.9f,.12f,new Color(.66f,.78f,.49f),.008f);
            Art.Ring("Garden inner motif",transform,3.6f,.045f,new Color(.84f,.79f,.56f),.009f);
            Art.Ring("Center circle",transform,1.25f,.045f,new Color(.84f,.79f,.56f),.01f);
            var random = new System.Random(76);
            for (int i = 0; i < 36; i++)
            {
                float angle = i * Mathf.PI * 2 / 36;
                Vector3 position = new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle)) * 12.4f;
                if (i % 5 == 0) Tree(position, i);
                else if (i % 3 == 0) Rock(position);
                else Flowers(position, random);
            }
            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI * 2 / 12;
                var stone = Art.Shape("Boundary stepping stone", PrimitiveType.Cube, transform,new Vector3(Mathf.Sin(a)*11.40f,.025f,Mathf.Cos(a)*11.40f),new Vector3(.65f,.11f,.36f),new Color(.91f,.88f,.73f));
                stone.transform.rotation = Quaternion.Euler(0,a*Mathf.Rad2Deg,0);
            }
            // Small spawn dais, on the floor, never an invisible movement obstacle.
            Art.Ring("Player spawn marker",transform,.85f,.07f,Art.Mint,.014f).transform.position = new Vector3(0,0,-4);
            for (int i = -1; i <= 1; i += 2)
            {
                Art.Shape("Garden lantern",PrimitiveType.Cube,transform,new Vector3(i*4,.38f,11.8f),new Vector3(.24f,.76f,.24f),new Color(.40f,.48f,.35f));
                Art.Shape("Lantern cap",PrimitiveType.Sphere,transform,new Vector3(i*4,.88f,11.8f),new Vector3(.50f,.48f,.50f),Art.Cream);
            }
        }

        void Tree(Vector3 position, int index)
        {
            Transform root = new GameObject("Candy garden tree").transform; root.SetParent(transform); root.position = position;
            Art.Shape("Trunk",PrimitiveType.Cylinder,root,new Vector3(0,.75f,0),new Vector3(.26f,.75f,.26f),new Color(.58f,.42f,.26f));
            Color leaf = index%2==0 ? new Color(.32f,.63f,.47f) : new Color(.48f,.71f,.37f);
            Art.Shape("Round canopy",PrimitiveType.Sphere,root,new Vector3(0,1.97f,0),new Vector3(1.65f,1.85f,1.55f),leaf);
            Art.Shape("Round canopy side",PrimitiveType.Sphere,root,new Vector3(.48f,1.55f,.12f),Vector3.one*1.05f,leaf);
            Art.Shape("Golden fruit",PrimitiveType.Sphere,root,new Vector3(-.48f,1.64f,-.51f),Vector3.one*.23f,Art.Yellow);
        }
        void Rock(Vector3 position)
        {
            Art.Shape("Smooth garden rock",PrimitiveType.Sphere,transform,position+Vector3.up*.22f,new Vector3(1.0f,.54f,.70f),new Color(.66f,.74f,.67f));
            Art.Shape("Small garden rock",PrimitiveType.Sphere,transform,position+new Vector3(.48f,.12f,.20f),new Vector3(.45f,.28f,.44f),new Color(.76f,.81f,.70f));
        }
        void Flowers(Vector3 position, System.Random random)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 p = position+new Vector3((float)random.NextDouble()*.65f-.3f,0,(float)random.NextDouble()*.6f-.3f);
                Art.Shape("Flower stem",PrimitiveType.Capsule,transform,p+Vector3.up*.23f,new Vector3(.045f,.20f,.045f),Art.Mint);
                Color petal = i%2==0 ? new Color(1,.65f,.62f) : Art.Cream;
                for (int k=0;k<5;k++) { float a=k*Mathf.PI*2/5; Art.Shape("Petal",PrimitiveType.Sphere,transform,p+new Vector3(Mathf.Sin(a)*.105f,.43f,Mathf.Cos(a)*.105f),new Vector3(.17f,.07f,.17f),petal); }
                Art.Shape("Flower center",PrimitiveType.Sphere,transform,p+Vector3.up*.46f,Vector3.one*.11f,Art.Yellow);
            }
        }
    }
}

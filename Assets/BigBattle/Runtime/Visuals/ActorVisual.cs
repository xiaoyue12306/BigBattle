using UnityEngine;

namespace BigBattle
{
    public sealed class ActorVisual : MonoBehaviour
    {
        public Transform Body, LeftHand, RightHand;
        public Renderer[] Renderers;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        float flashUntil, swingUntil, moveAmount, slipUntil;
        bool spinning;
        float spinAngle;

        public static ActorVisual Create(Transform parent, int style)
        {
            var root = new GameObject("Animated visual"); root.transform.SetParent(parent, false);
            var visual = root.AddComponent<ActorVisual>();
            visual.Body = new GameObject("Body rig").transform; visual.Body.SetParent(root.transform, false);
            if (style == 0) visual.BuildBanana(); else if (style == 1) visual.BuildMochi(); else visual.BuildDummy();
            visual.Renderers = root.GetComponentsInChildren<Renderer>(); return visual;
        }

        void BuildBanana()
        {
            Vector3[] curve = { new Vector3(-.30f,.47f,0), new Vector3(-.13f,.63f,0), new Vector3(.04f,.93f,0), new Vector3(.16f,1.27f,0), new Vector3(.13f,1.60f,0), new Vector3(.02f,1.89f,0), new Vector3(-.16f,2.08f,0) };
            Art.MeshObject("Banana golden skin", Body, Art.CurvedTube(curve, new[] {.075f,.22f,.31f,.33f,.29f,.18f,.065f}, 16), Art.Yellow);
            Art.Shape("Stem", PrimitiveType.Capsule, Body, new Vector3(-.18f,2.09f,0), new Vector3(.14f,.10f,.14f), new Color(.38f,.46f,.16f)).transform.localRotation = Quaternion.Euler(0,0,30);
            Art.Shape("Tip", PrimitiveType.Sphere, Body, new Vector3(-.31f,.46f,0), Vector3.one * .14f, new Color(.42f,.28f,.17f));
            Eye(Body, new Vector3(-.015f,1.46f,.286f), .095f);
            Eye(Body, new Vector3(.29f,1.46f,.25f), .095f);
            Art.Shape("Cheek L", PrimitiveType.Sphere, Body, new Vector3(-.11f,1.33f,.28f), new Vector3(.13f,.07f,.025f), new Color(1,.48f,.38f));
            Art.Shape("Cheek R", PrimitiveType.Sphere, Body, new Vector3(.35f,1.33f,.22f), new Vector3(.10f,.06f,.025f), new Color(1,.48f,.38f));
            Art.Shape("Smile", PrimitiveType.Capsule, Body, new Vector3(.14f,1.29f,.32f), new Vector3(.09f,.035f,.025f), Art.Ink).transform.localRotation = Quaternion.Euler(0,0,90);
            Art.Shape("Mint scarf", PrimitiveType.Capsule, Body, new Vector3(.11f,1.05f,0), new Vector3(.66f,.095f,.63f), Art.Mint);
            Art.Shape("Scarf tail", PrimitiveType.Cube, Body, new Vector3(.40f,1.01f,-.10f), new Vector3(.38f,.10f,.16f), Art.Mint).transform.localRotation = Quaternion.Euler(0,15,-15);
            LeftHand = Hand(-1); RightHand = Hand(1);
            for (int i = -1; i <= 1; i += 2)
            {
                Art.Shape("Little boot", PrimitiveType.Capsule, Body, new Vector3(i*.23f,.22f,.06f), new Vector3(.25f,.18f,.38f), Art.Ink);
            }
        }

        Transform Hand(int side)
        {
            var pivot = new GameObject("Peel blade hand").transform; pivot.SetParent(Body, false); pivot.localPosition = new Vector3(side*.49f,.91f,.03f);
            Art.Shape("Mitten", PrimitiveType.Sphere, pivot, Vector3.zero, Vector3.one*.23f, Art.Cream);
            Vector3[] centers = { Vector3.zero, new Vector3(side*.12f,.22f,.04f), new Vector3(side*.16f,.48f,.13f), new Vector3(side*.12f,.71f,.26f) };
            Art.MeshObject("Curved peel blade", pivot, Art.CurvedTube(centers, new[] {.09f,.10f,.065f,.005f}, 6), Art.Cream);
            return pivot;
        }

        void BuildMochi()
        {
            Art.Shape("Mochi body", PrimitiveType.Sphere, Body, new Vector3(0,.65f,0), new Vector3(1.0f,1.05f,.9f), new Color(1,.58f,.62f));
            for (int side = -1; side <= 1; side += 2)
            {
                Art.Shape("Tiny foot", PrimitiveType.Sphere, Body, new Vector3(side*.29f,.15f,.12f), new Vector3(.28f,.22f,.35f), new Color(.67f,.26f,.35f));
                Art.Shape("Leaf", PrimitiveType.Capsule, Body, new Vector3(side*.18f,1.26f,0), new Vector3(.19f,.32f,.08f), Art.Mint).transform.localRotation = Quaternion.Euler(0,0,side*-40);
                Eye(Body, new Vector3(side*.18f,.77f,.42f), .09f);
            }
            Art.Shape("Mouth", PrimitiveType.Sphere, Body, new Vector3(0,.59f,.445f), new Vector3(.10f,.08f,.025f), Art.Ink);
        }

        void BuildDummy()
        {
            Art.Shape("Post", PrimitiveType.Cylinder, Body, new Vector3(0,.62f,0), new Vector3(.18f,.60f,.18f), new Color(.55f,.34f,.19f));
            Art.Shape("Straw body", PrimitiveType.Capsule, Body, new Vector3(0,1.02f,0), new Vector3(.80f,.55f,.70f), new Color(.88f,.65f,.32f));
            Art.Shape("Crossbar", PrimitiveType.Capsule, Body, new Vector3(0,.99f,0), new Vector3(.17f,.74f,.17f), new Color(.55f,.34f,.19f)).transform.localRotation = Quaternion.Euler(0,0,90);
            Art.Shape("Hat", PrimitiveType.Cylinder, Body, new Vector3(0,1.59f,0), new Vector3(1.10f,.06f,.90f), Art.Cream);
            Eye(Body, new Vector3(-.17f,1.24f,.32f), .07f); Eye(Body, new Vector3(.17f,1.24f,.32f), .07f);
            Art.Ring("Dummy target", transform, .67f, .065f, new Color(.89f,.67f,.33f));
        }

        static void Eye(Transform parent, Vector3 position, float size)
        {
            Art.Shape("Eye", PrimitiveType.Sphere, parent, position, new Vector3(size,size*1.35f,size*.50f), Art.Ink);
            Art.Shape("Eye glint", PrimitiveType.Sphere, parent, position + new Vector3(-size*.15f,size*.25f,size*.26f), Vector3.one*size*.25f, Color.white);
        }

        public void Animate(float movement, bool spin) { moveAmount = movement; spinning = spin; }
        public void Swing() { swingUntil = Time.time + .18f; }
        public void Flash() { flashUntil = Time.time + .10f; }
        public void Slip(float duration) { slipUntil = Time.time + duration; }
        void LateUpdate()
        {
            float bounce = Mathf.Sin(Time.time * (moveAmount > .1f ? 14 : 3)) * (moveAmount > .1f ? .065f : .025f);
            Body.localPosition = new Vector3(0,bounce,0);
            float lean = moveAmount * 8;
            if (spinning) spinAngle += Time.deltaTime * 1100; else spinAngle = Mathf.LerpAngle(spinAngle,0,Time.deltaTime*18);
            Body.localRotation = Quaternion.Euler(lean,spinAngle,Mathf.Sin(Time.time*10)*moveAmount*4);
            if(Time.time<slipUntil)
            {
                Body.localRotation=Quaternion.Euler(0,0,72);
                Body.localPosition=new Vector3(0,-.20f+Mathf.Sin(Time.time*18)*.025f,0);
            }
            float swing = Mathf.Clamp01((swingUntil-Time.time)/.18f);
            if (LeftHand != null) LeftHand.localRotation = Quaternion.Euler(-swing*100,0,-swing*25);
            if (RightHand != null) RightHand.localRotation = Quaternion.Euler(-swing*125,0,swing*25);
            block.SetColor("_EmissionColor", Time.time < flashUntil ? new Color(.55f,.5f,.4f) : Color.black);
            // A color multiplier flash preserves each original material.
            foreach (Renderer item in Renderers)
            {
                if (item == null) continue;
                block.SetColor("_Color", Time.time < flashUntil ? Color.white : item.sharedMaterial.color);
                item.SetPropertyBlock(block);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;
using System;

public class TestGL : MonoBehaviour {
    struct TransformPair
    {
        public Transform t1, t2;

        public TransformPair(Transform t1, Transform t2)
        {
            this.t1 = t1;
            this.t2 = t2;
        }
    }

    public Material eyeMaterial, fuzzMaterial, lineMaterial;
    Material current = null;
    public GameObject root;
    public new Camera camera;
    Dictionary<Texture, List<Transform>> balls = new Dictionary<Texture, List<Transform>>();
    Dictionary<Texture, List<TransformPair>> lines = new Dictionary<Texture, List<TransformPair>>();
    Dictionary<Texture, List<Paintball>> paintballs = new Dictionary<Texture, List<Paintball>>();
    List<Eye> eyes = new List<Eye>();

    PetzAnimationDecoder decoder;
    LnzDecoder lnzDecoder;
    int animation = 0;

    void Start()
    {
        camera = GetComponent<Camera>();
        parseTree(root);
        string animal = "CAT";
        decoder = new PetzAnimationDecoder(File.ReadAllBytes(Application.streamingAssetsPath+"/" + animal));
        for(int i = 0; i <= 444; i++)
        {
            decoder.addAnimation(i, File.ReadAllBytes(Application.streamingAssetsPath + "/" + animal + "_BHT/" + animal + i));
        }
        lnzDecoder = new LnzDecoder(File.ReadAllLines(Application.streamingAssetsPath + "/" + animal + ".lnz"),decoder);
    }

    void OnPostRender()
    {
        GL.PushMatrix();
        GL.modelview = Matrix4x4.identity;
        GL.LoadProjectionMatrix(camera.projectionMatrix);
       
        foreach(var pair in balls)
        {
            fuzzMaterial.SetTexture("_MainTex", pair.Key);
            fuzzMaterial.SetVector("_Up", root.transform.up);
            fuzzMaterial.SetVector("_Right", root.transform.right);
            fuzzMaterial.SetFloat("_Outline", 0F);
            fuzzMaterial.SetVector("_OutlineDirection", -root.transform.up);
            fuzzMaterial.SetPass(0);
            GL.Begin(GL.QUADS);
            foreach(var ball in pair.Value)
            {
                drawFuzzball(ball.transform.localScale.x / 2, ball.transform.position);
            }
            GL.End();
        }
        foreach(var pair in lines)
        {
            lineMaterial.SetTexture("_MainTex", pair.Key);
            lineMaterial.SetPass(0);
            GL.Begin(GL.QUADS);
            foreach(var line in pair.Value)
            {
                drawLine(line.t1.localScale.x / 2, line.t2.localScale.x / 2, line.t1.position, line.t2.position);
            }
            GL.End();
        }
        foreach(var pair in paintballs)
        {
            fuzzMaterial.SetTexture("_MainTex", pair.Key);
            fuzzMaterial.SetVector("_Up", root.transform.up);
            fuzzMaterial.SetVector("_Right", root.transform.right);
            fuzzMaterial.SetFloat("_Outline", 0F);
            fuzzMaterial.SetVector("_OutlineDirection", -root.transform.up);
            fuzzMaterial.SetPass(0);
            GL.Begin(GL.QUADS);
            foreach (var paintball in pair.Value)
            {
                Quaternion q = paintball.gameObject.transform.rotation;
                drawPaintball(paintball.transform.localScale.x / 2, paintball.transform.position,q*paintball.direction,paintball.amount);
            }
            GL.End();
        }
        foreach(var eye in eyes)
        {
            var t = eye.gameObject.transform;
            drawEye(t.localScale.x / 2, eye.closedness, t.position, t.rotation*eye.eyelid, t.forward, t.up, t.right,eye.iris,eye.eyelidColor);
        }

        try
        {
            fuzzMaterial.SetVector("_Up", root.transform.up);
            fuzzMaterial.SetVector("_Right", root.transform.right);
            fuzzMaterial.SetFloat("_Outline", 0F);
            fuzzMaterial.SetVector("_OutlineDirection", -root.transform.up);
            fuzzMaterial.SetPass(0);
            GL.Begin(GL.QUADS);
            int frame = Mathf.RoundToInt(Time.timeSinceLevelLoad * 10) % decoder.animations[animation].frames.Count;
            for (int i = 0; i < decoder.animations[animation].frames[frame].balls.Length; i++)
            {
                var ball = decoder.animations[animation].frames[frame].balls[i];
                drawFuzzball(decoder.ballSizes[i] / 2, new Vector3(ball.x, -ball.y, ball.z));
            }
            if(!Input.GetKey(KeyCode.Space)) for (int i = 0; i < lnzDecoder.addballs.Count; i++)
            {
                var ball = lnzDecoder.addballs[i];
                var parent = decoder.animations[animation].frames[frame].balls[ball.parent];
                drawFuzzball(ball.size / 2, new Vector3(ball.x + parent.x, -(ball.y + parent.y), ball.z + parent.z));
            }
            GL.End();

            //lineMaterial.SetTexture("_MainTex", pair.Key);
            lineMaterial.SetPass(0);
            GL.Begin(GL.QUADS);
            //Debug.Log("Drawing " + lnzDecoder.lines.Count + " lines");
            foreach (var line in lnzDecoder.lines)
            {
                int size1, size2;
                Vector3 parent1, parent2;
                if(line.parent1<decoder.numBalls)
                {
                    var parent = decoder.animations[animation].frames[frame].balls[line.parent1];
                    size1 = decoder.ballSizes[line.parent1];
                    parent1 = new Vector3(parent.x, -parent.y, parent.z);
                } else
                {
                    var parent = lnzDecoder.addballs[line.parent1 - decoder.numBalls];
                    var parentParent = decoder.animations[animation].frames[frame].balls[parent.parent];
                    size1 = parent.size;
                    parent1 = new Vector3(parent.x+parentParent.x, -(parent.y+parentParent.y), parent.z+parentParent.z);
                }
                if (line.parent2 < decoder.numBalls)
                {
                    var parent = decoder.animations[animation].frames[frame].balls[line.parent2];
                    size2 = decoder.ballSizes[line.parent2];
                    parent2 = new Vector3(parent.x, -parent.y, parent.z);
                }
                else
                {
                    var parent = lnzDecoder.addballs[line.parent2 - decoder.numBalls];
                    var parentParent = decoder.animations[animation].frames[frame].balls[parent.parent];
                    size2 = parent.size;
                    parent2 = new Vector3(parent.x + parentParent.x, -(parent.y + parentParent.y), parent.z + parentParent.z);
                }
                drawLine(size1/2, size2 / 2, parent1, parent2);
            }
            GL.End();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        GL.PopMatrix();
    }

    void parseTree(GameObject g)
    {
        SphereCollider sphereCollider = g.GetComponent<SphereCollider>();
        if (sphereCollider)
        {
            Texture t = g.GetComponent<MeshRenderer>().material.mainTexture;
            if(t!=null)
            {
                if (!balls.ContainsKey(t))
                {
                    balls.Add(t, new List<Transform>());
                }
                var list = balls[t];
                list.Add(g.transform);
            }
        }
        Line line = g.GetComponent<Line>();
        if (line != null)
        {
            GameObject other = line.other;
            if (other == null) other = g.transform.parent.gameObject;
            if(!lines.ContainsKey(line.texture))
            {
                lines.Add(line.texture, new List<TransformPair>());
            }
            lines[line.texture].Add(new TransformPair(g.transform, other.transform));
        }
        Eye eye = g.GetComponent<Eye>();
        if (eye != null)
        {
            eyes.Add(eye);
        }
        var paintballList = g.GetComponents<Paintball>();
        foreach(var paintball in paintballList)
        {
            if (!paintballs.ContainsKey(paintball.texture))
            {
                paintballs.Add(paintball.texture, new List<Paintball>());
            }
            paintballs[paintball.texture].Add(paintball);
        }
        for (int i = 0; i < g.transform.childCount; i++)
        {
            parseTree(g.transform.GetChild(i).gameObject);
        }
    }

    void drawEye(float size, float closedness, Vector3 position, Vector3 eyelid, Vector3 direction, Vector3 up, Vector3 right, Texture iris, Color eyelidColor)
    {
        Vector4 v = new Vector4(position.x, position.y, position.z, 1);
        v = camera.worldToCameraMatrix * v;
        eyeMaterial.SetVector("_Eyelid", camera.worldToCameraMatrix * eyelid);
        eyeMaterial.SetVector("_Direction", camera.worldToCameraMatrix * direction);
        eyeMaterial.SetFloat("_Closedness", closedness);
        eyeMaterial.SetVector("_Up", camera.worldToCameraMatrix * up);
        eyeMaterial.SetVector("_Right", camera.worldToCameraMatrix * right);
        eyeMaterial.SetPass(0);
        GL.Begin(GL.QUADS);
        
        GL.TexCoord2(0, 0);
        GL.Vertex3(v.x - size, v.y - size, v.z);
        GL.TexCoord2(0, 1);
        GL.Vertex3(v.x - size, v.y + size, v.z);
        GL.TexCoord2(1, 1);
        GL.Vertex3(v.x + size, v.y + size, v.z);
        GL.TexCoord2(1, 0);
        GL.Vertex3(v.x + size, v.y - size, v.z);
        GL.End();
    }

    void drawFuzzball(float size, Vector3 position)
    {
        Vector4 v = new Vector4(position.x, position.y, position.z, 1);
        v = camera.worldToCameraMatrix * v;

        float angle = camera.transform.rotation.eulerAngles.z;
        Quaternion q = Quaternion.AngleAxis(angle, camera.transform.forward);

        Vector2 uv = q * new Vector2(-.5F, -.5F);
        GL.Color(new Color(1, 1, 1,0));
        GL.TexCoord2(uv.x + .5F, uv.y + .5F);
        GL.Vertex3(v.x - size, v.y - size, v.z);
        uv = q * new Vector2(-.5F, .5F);
        GL.Color(new Color(1, 1, 1, 0));
        GL.TexCoord2(uv.x + .5F, uv.y + .5F);
        GL.Vertex3(v.x - size, v.y + size, v.z);
        uv = q * new Vector2(.5F, .5F);
        GL.Color(new Color(1, 1, 1, 0));
        GL.TexCoord2(uv.x + .5F, uv.y + .5F);
        GL.Vertex3(v.x + size, v.y + size, v.z);
        uv = q * new Vector2(.5F, -.5F);
        GL.Color(new Color(1, 1, 1, 0));
        GL.TexCoord2(uv.x + .5F, uv.y + .5F);
        GL.Vertex3(v.x + size, v.y - size, v.z);
    }

    void drawPaintball(float size, Vector3 position, Vector4 direction, float amount)
    {
        Vector4 v = new Vector4(position.x, position.y, position.z, 1);
        v = camera.worldToCameraMatrix * v;

        float angle = camera.transform.rotation.eulerAngles.z;
        Quaternion q = Quaternion.AngleAxis(angle, camera.transform.forward);

        direction = camera.worldToCameraMatrix * direction;
        direction = (direction+new Vector4(1,1,1,0))/ 2;
        Color paintball = new Color(direction.x, direction.y, direction.z, amount);

        Vector2 uv = q * new Vector2(-.5F, -.5F);
        GL.Color(paintball);
        GL.TexCoord2(uv.x + .5F, uv.y + .5F);
        GL.Vertex3(v.x - size, v.y - size, v.z);
        uv = q * new Vector2(-.5F, .5F);
        GL.Color(paintball);
        GL.TexCoord2(uv.x + .5F, uv.y + .5F);
        GL.Vertex3(v.x - size, v.y + size, v.z);
        uv = q * new Vector2(.5F, .5F);
        GL.Color(paintball);
        GL.TexCoord2(uv.x + .5F, uv.y + .5F);
        GL.Vertex3(v.x + size, v.y + size, v.z);
        uv = q * new Vector2(.5F, -.5F);
        GL.Color(paintball);
        GL.TexCoord2(uv.x + .5F, uv.y + .5F);
        GL.Vertex3(v.x + size, v.y - size, v.z);
    }

    void drawLine(float size1, float size2, Vector3 ball1, Vector3 ball2)
    {
        Vector4 v1 = camera.worldToCameraMatrix * new Vector4(ball1.x, ball1.y, ball1.z, 1);
        Vector4 v2 = camera.worldToCameraMatrix * new Vector4(ball2.x, ball2.y, ball2.z, 1);
        Vector4 p1 = camera.WorldToScreenPoint(ball1);
        p1.w = 1;
        Vector4 p2 = camera.WorldToScreenPoint(ball2);
        p2.w = 1;
        float angle = Mathf.Atan2(v2.y - v1.y, v2.x - v1.x)+Mathf.PI/2;
        Vector2 d = new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*.95F;

        size1 *= .95F;
        size2 *= .95F;

        if(v1.z<v2.z)
        {
            GL.TexCoord2(0, 0);
            GL.Vertex3(v1.x + d.x * size1, v1.y + d.y * size1, v1.z);
            GL.TexCoord2(0, 1);
            GL.Vertex3(v1.x - d.x * size1, v1.y - d.y * size1, v1.z);
            GL.TexCoord2(1, 1);
            Vector4 a1 = camera.WorldToScreenPoint(camera.cameraToWorldMatrix*new Vector4(v2.x - d.x * size2, v2.y - d.y * size2, v2.z,1));
            a1 = new Vector4(a1.x, a1.y, p1.z);
            a1 = camera.ScreenToWorldPoint(a1);
            a1.w = 1;
            a1 = camera.worldToCameraMatrix * a1;
            GL.Vertex3(a1.x, a1.y, a1.z);
            GL.TexCoord2(1, 0);
            Vector4 a2 = camera.WorldToScreenPoint(camera.cameraToWorldMatrix*new Vector4(v2.x + d.x * size2, v2.y + d.y * size2, v2.z,1));
            a2 = new Vector3(a2.x, a2.y, p1.z);
            a2 = camera.ScreenToWorldPoint(a2);
            a2.w = 1;
            a2 = camera.worldToCameraMatrix * a2;
            GL.Vertex3(a2.x, a2.y, a2.z);
        } else
        {
            GL.TexCoord2(0, 0);
            Vector4 a1 = camera.WorldToScreenPoint(camera.cameraToWorldMatrix * new Vector4(v1.x + d.x * size1, v1.y + d.y * size1, v1.z, 1));
            a1 = new Vector4(a1.x, a1.y, p2.z);
            a1 = camera.ScreenToWorldPoint(a1);
            a1.w = 1;
            a1 = camera.worldToCameraMatrix * a1;
            GL.Vertex3(a1.x, a1.y,a1.z);
            GL.TexCoord2(0, 1);
            Vector4 a2 = camera.WorldToScreenPoint(camera.cameraToWorldMatrix * new Vector4(v1.x - d.x * size1, v1.y - d.y * size1, v1.z, 1));
            a2 = new Vector4(a2.x, a2.y, p2.z);
            a2 = camera.ScreenToWorldPoint(a2);
            a2.w = 1;
            a2 = camera.worldToCameraMatrix * a2;
            GL.Vertex3(a2.x,a2.y,a2.z);
            GL.TexCoord2(1, 1);
            GL.Vertex3(v2.x - d.x * size2, v2.y - d.y * size2, v2.z);
            GL.TexCoord2(1, 0);
            GL.Vertex3(v2.x + d.x * size2, v2.y + d.y * size2, v2.z);
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            animation = (animation + 1) % decoder.animations.Count;
        }
    }
}

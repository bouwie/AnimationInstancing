using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;

[RequireComponent(typeof(Animator))]
public class AICreateAnimations : MonoBehaviour
{
    public Mesh[] meshesCreated;

    public string saveDirectory;

    public SaveMode saveMode;

    public string[] animationForSave = new string[1];

    private Animator animator;

    private bool combined;

    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private float[] maxMagnitudes;
    private float maxCombinedMagnitude;
    private int x, y;
    private float currentTime;
    private int currentIndex;

    private bool isFindingMaxMagnitudes;
    private bool isBaking;

    public float clipLenght;
    private Texture2D texture;

    public bool canCreate { get; private set; } = false;

    public enum SaveMode
    {
        SingleAnimation,
        Multiple,
        All
    }

    private void Start() {
        canCreate = true;
    }

    public void SetAllAnimations() {
        Animator animator = GetComponent<Animator>();

        // animator.runtimeAnimatorController.animationClips;
    }

    public void GenerateAnimations() {
        if(!canCreate) {
            return;
        }

        if(saveDirectory == string.Empty) {
            Debug.LogError("Save directory is not specified!");
            return;
        }

        StopAllCoroutines();

        animator = GetComponent<Animator>();
        skinnedMeshRenderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();

        currentIndex = 0;
        MoveToNextAnimation(animationForSave[currentIndex]);

        isFindingMaxMagnitudes = true;

        StartCoroutine(BakeAndFind());
    }

    private void MoveToNextAnimation(string _name) {
        maxMagnitudes = new float[skinnedMeshRenderers.Length];
        maxCombinedMagnitude = 0;

        x = 0;
        y = 0;

        currentTime = 0;

        animator.Play(_name, 0, 0);
        animator.Update(0);
    }

    //private void Update() {
    //    if(!isFindingMaxMagnitudes && !isBaking) {
    //        return;
    //    }

    //    if(isFindingMaxMagnitudes) {
    //        FindMaxMagnitudes(skinnedMeshRenderers);
    //        if(currentTime > clipLenght) {
    //            StartBake();
    //        }
    //    }

    //    if(isBaking) {
    //        PaintFrameToTexture();
    //        if(currentTime > clipLenght) {           
    //            if(currentIndex + 1 < animationForSave.Length) {
    //                currentIndex++;
    //                MoveToNextAnimation(animationForSave[currentIndex]);
    //            } else {
    //                EndBake();
    //            }
    //        }
    //    }
    //    currentTime += 0.01f;
    //    animator.Play(animationForSave[currentIndex], 0, currentTime / clipLenght);
    //}

    private IEnumerator BakeAndFind() {
        while(isBaking || isFindingMaxMagnitudes) {

            if(isFindingMaxMagnitudes) {
                FindMaxMagnitudes(skinnedMeshRenderers);
                if(currentTime > clipLenght) {
                    StartBake();
                }
            }
            if(isBaking) {
                PaintFrameToTexture();
                if(currentTime > clipLenght) {
                    if(currentIndex + 1 < animationForSave.Length) {
                        currentIndex++;
                       MoveToNextAnimation(animationForSave[currentIndex]);
                    } else {
                        EndBake();
                    }
                }
            }

            currentTime += 0.01f;
            animator.Play(animationForSave[currentIndex], 0, currentTime / clipLenght);
            yield return new WaitForEndOfFrame();
        }
    }
        
    private void FindMaxMagnitudes(SkinnedMeshRenderer[] _skinnedMeshes) {

        int c = 0;
        foreach(SkinnedMeshRenderer skinnedMesh in _skinnedMeshes) {

            List<Vector3> firstVertices = new List<Vector3>();
            meshesCreated[c].GetVertices(firstVertices);
            

            Mesh mesh = new Mesh();
            skinnedMesh.BakeMesh(mesh);

            List<Vector3> vertices = new List<Vector3>();

            mesh.GetVertices(vertices);

            for(int i = 0; i < vertices.Count; i++) {
                float currentMagnitude = (vertices[i] - firstVertices[i]).magnitude;
                // float currentMagnitude = vertices[i].magnitude;

                if(currentMagnitude > maxMagnitudes[c]) {
                    maxMagnitudes[c] = currentMagnitude;
                }
            }


            c++;
        }

        if(combined) {
            for(int i = 0; i < maxMagnitudes.Length; i++) {
                if(maxMagnitudes[i] > maxCombinedMagnitude) {
                    maxCombinedMagnitude = maxMagnitudes[i];
                }
            }
        }
    }

    private void StartBake() {
        isFindingMaxMagnitudes = false;

        x = 0;
        y = 0;

        currentTime = 0;
        animator.Play(animationForSave[currentIndex], 0, 0);
        // clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        MakeTexture(skinnedMeshRenderers);

        isBaking = true;
    }

    private void MakeTexture(SkinnedMeshRenderer[] _skinnedMeshes) {
        int vertices = 0;
        int numberOfFrames = Mathf.RoundToInt(clipLenght / 0.01f) + 1;

        CombineInstance[] combine = new CombineInstance[skinnedMeshRenderers.Length];

        int i = 0;
        foreach(SkinnedMeshRenderer skinnedMesh in _skinnedMeshes) {
            Mesh mesh = new Mesh();
            skinnedMesh.BakeMesh(mesh);

            if(!combined) {
                vertices += mesh.vertexCount;
            } else {
                combine[i].mesh = mesh;
                combine[i].transform = skinnedMesh.localToWorldMatrix;
            }
            i++;
        }

        if(combined) {
            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combine, false);
            vertices = combinedMesh.vertexCount;
            //    combinedFirstMesh = combinedMesh;
        }

        Debug.Log("Vert Amount: " + vertices + "  Frames:" + numberOfFrames);
        texture = new Texture2D(vertices + 2, numberOfFrames);
        Debug.Log("width: " + texture.width + "  height:" + texture.height);
    }

    private void PaintFrameToTexture() {

        int c = 0;

        CombineInstance[] combine = new CombineInstance[skinnedMeshRenderers.Length];

        List<Vector3> firstVertices = new List<Vector3>();
        List<Vector3> vertices = new List<Vector3>();


        foreach(SkinnedMeshRenderer skinnedMesh in skinnedMeshRenderers) {

            Mesh mesh = new Mesh();
            skinnedMesh.BakeMesh(mesh);
            mesh.GetVertices(vertices);


            meshesCreated[c].GetVertices(firstVertices);


            if(!combined) {
                for(int i = 0; i < vertices.Count; i++) {
                    texture.SetPixel(x, y, PositionToRGBA(vertices[i] - firstVertices[i], maxMagnitudes[c]));
                    x++;
                }

            } else {
                combine[c].mesh = mesh;
                combine[c].transform = skinnedMesh.localToWorldMatrix;
            }
            c++;
        }

        if(combined) {
            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combine, false);

            List<Vector3> combinedVertices = new List<Vector3>();
            combinedMesh.GetVertices(combinedVertices);


            List<Vector3> combinedFirstVertices = new List<Vector3>();
          //  combinedFirstMesh.GetVertices(combinedFirstVertices);

            for(int i = 0; i < combinedVertices.Count; i++) {
                if(y > 0) {
                    texture.SetPixel(x, y, PositionToRGBA(combinedVertices[i] - combinedFirstVertices[i], maxCombinedMagnitude));
                } else {
                    texture.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
                x++;
            }
        }


        x = 0;
        y++;
    }

    private Color PositionToRGBA(Vector3 _pos, float _maxMagnitude) {
        float length = _pos.magnitude;
        Vector3 norm = _pos.normalized;

        return new Color((norm.x + 1) / 2, (norm.y + 1) / 2, (norm.z + 1) / 2, length / _maxMagnitude);
    }

    private void EndBake() {
        StopBake();
        Debug.Log("Writing to Texture...");

        string texPath = saveDirectory + "/" + animationForSave[currentIndex] + ".png";
        File.WriteAllBytes(texPath, texture.EncodeToPNG());


        AIAnimation animation = ScriptableObject.CreateInstance<AIAnimation>();
        if(!combined) {
            animation.maxMagnitudes = maxMagnitudes;
        } else {
            animation.maxMagnitudes = new float[] { maxCombinedMagnitude };
        }
        animation.length = clipLenght;

        if(File.Exists(texPath)) {
            byte[] texData;
            texData = File.ReadAllBytes(texPath);

            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(texData);
            animation.animationTexture = tex;
                }
        //   animation.startMeshes = firstMeshes;

        string absolutePath = saveDirectory + "/" + animationForSave[currentIndex] + ".asset";
        string relativePath = Application.dataPath;

        if(absolutePath.StartsWith(Application.dataPath)) {
            relativePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
        }

        AssetDatabase.CreateAsset(animation, relativePath);

        AssetDatabase.SaveAssets();

        Debug.Log("Finished Writing");
    }

    public void StopBake() {
        isBaking = false;
        isFindingMaxMagnitudes = false;
    }

    private void OnDisable() {
        StopBake();
    }
}
#endif
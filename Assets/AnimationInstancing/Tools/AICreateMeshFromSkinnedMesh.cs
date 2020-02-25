using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AnimationInstancing
{

    public class AICreateMeshFromSkinnedMesh : MonoBehaviour
    {
        [SerializeField] private bool combineMeshes;

        [HideInInspector] public string saveDirectory;

        public Mesh[] Transform() {
            SkinnedMeshRenderer[] skinnedMeshRenderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            CombineInstance[] combine = new CombineInstance[skinnedMeshRenderers.Length];
            List<Material> materials = new List<Material>();
            Mesh[] meshes = new Mesh[skinnedMeshRenderers.Length];

            int x = 0;

            for(int i = 0; i < skinnedMeshRenderers.Length; i++) {
                SkinnedMeshRenderer skinnedMesh = skinnedMeshRenderers[i];


                string absolutePath = saveDirectory + "/" + skinnedMesh.gameObject.name + "Mesh.asset";
                string relativePath = Application.dataPath;

                if(absolutePath.StartsWith(Application.dataPath)) {
                    relativePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
                }

                Mesh mesh = new Mesh();

                skinnedMesh.BakeMesh(mesh);


                foreach(Material material in skinnedMesh.sharedMaterials) {
                    if(!materials.Contains(material)) {
                        materials.Add(material);
                    }
                }

                if(!combineMeshes) {

                    MeshFilter meshFilter = skinnedMesh.gameObject.AddComponent<MeshFilter>();
                    meshFilter.mesh = mesh;

                    MeshRenderer meshRenderer = skinnedMesh.gameObject.AddComponent<MeshRenderer>();
                    meshRenderer.sharedMaterials = skinnedMesh.sharedMaterials;

                }


                combine[i].mesh = mesh;
                combine[i].transform = skinnedMesh.transform.localToWorldMatrix;

                DestroyImmediate(skinnedMesh);

                Vector2[] uvs = new Vector2[mesh.vertexCount];
                for(int v = 0; v < uvs.Length; v++) {
                    uvs[v] = new Vector2(x, 0);
                    x++;
                }

                mesh.uv2 = uvs;



                #if UNITY_EDITOR
                AssetDatabase.CreateAsset(mesh, relativePath);
                AssetDatabase.SaveAssets();
                #endif

                meshes[i] = mesh;

            }

            if(combineMeshes) {
                MeshRenderer renderer = transform.gameObject.AddComponent<MeshRenderer>();
                MeshFilter filter = transform.gameObject.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                filter.sharedMesh = mesh;
                filter.sharedMesh.CombineMeshes(combine, false);
                renderer.sharedMaterials = materials.ToArray();

                Vector2[] uvs = new Vector2[mesh.vertexCount];
                x = 0;
                for(int v = 0; v < uvs.Length; v++) {
                    uvs[v] = new Vector2(x, 0);
                    x++;
                }
                mesh.uv2 = uvs;
                filter.sharedMesh = mesh;
            }
            return meshes;
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(AICreateMeshFromSkinnedMesh))]
public class AICreateMeshFromSkinnedMeshEditor : Editor
{
    private AICreateMeshFromSkinnedMesh meshCreator;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if(target != null) {
            meshCreator = (AICreateMeshFromSkinnedMesh)target;
        }

        GUILayout.Label("Save Directory: " + meshCreator.saveDirectory);
        if(GUILayout.Button("Set Save Directory")) {
            string saveDir = EditorUtility.OpenFolderPanel("Set Save Directory", System.Environment.CurrentDirectory, "");
            if(saveDir != string.Empty) {
                meshCreator.saveDirectory = saveDir;
            }
        }

        GUILayout.Space(5);

        if(GUILayout.Button("Transform")) {
            GameObject copy = GameObject.Instantiate(meshCreator.gameObject);
            copy.name = meshCreator.gameObject.name + " AI";

            copy.AddComponent<AIAnimator>();

            AICreateMeshFromSkinnedMesh meshCopyComp = copy.GetComponent<AICreateMeshFromSkinnedMesh>();

            Mesh[] newMeshes = meshCopyComp.Transform();

            DestroyImmediate(meshCopyComp);
            DestroyImmediate(copy.GetComponent<AICreateAnimations>());
            DestroyImmediate(copy.GetComponent<Animator>());

            AICreateAnimations createAnimations = meshCreator.GetComponent<AICreateAnimations>();
            if(createAnimations != null) {
                createAnimations.meshesCreated = newMeshes;
            }

          
        }
    }
}
#endif

}
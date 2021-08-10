using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;
using Unity.AI.Navigation.Editor;

public class NavMeshLinksPlacer : EditorWindow
{
    [SerializeField]
    private int meshWidth = 512;
    [SerializeField]
    private float navMeshLinkThickness = 10;
    [SerializeField]
    private float navMeshLinkWidth = 512;
    [SerializeField]
    private string startName = "LODMeshTerrain_";

    private GameObject[] meshTerrains;
    [SerializeField]
    private int totalCount = 0;
    [SerializeField]
    private int totalNavMeshLinks;

    int side;
    MeshTerrainSort[,] sortedMeshTerrains;

    [SerializeField]
    private bool showDebugInfo;
    [MenuItem("Selection/NavMeshLinksPlacer")]
    static void Init()
    {
        NavMeshLinksPlacer window = (NavMeshLinksPlacer)GetWindow(typeof(NavMeshLinksPlacer));
    }

    void OnGUI()
    {
        if (GUILayout.Button("Select NavMesh Surfaces (Generate if not exist)"))
        {
            SelectSurfaces();
        }
        meshWidth = (int)EditorGUILayout.IntField("meshWidth", meshWidth);
        navMeshLinkThickness = (float)EditorGUILayout.FloatField("navMeshLinkThickness", navMeshLinkThickness);
        navMeshLinkWidth = (float)EditorGUILayout.FloatField("navMeshLinkWidth", navMeshLinkWidth);
        if (showDebugInfo)
        {
            startName = (string)EditorGUILayout.TextField("nameStarts", startName);
            EditorGUILayout.IntField("totalFoundMeshes", totalCount);
            EditorGUILayout.IntField("totalNavMeshLinks", totalNavMeshLinks);

            if (GUILayout.Button("Find MeshTerrains"))
            {
                FindNavMeshes();
            }
            if (GUILayout.Button("Sort MeshTerrains"))
            {
                SortMeshTerrains();
            }
            if (GUILayout.Button("Create NavMesh Links"))
            {
                GenerateLinks();
            }

            if (sortedMeshTerrains != null)
            {
                for (int i = 0; i < side; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    for (int j = 0; j < side; j++)
                    {
                        EditorGUILayout.TextField(sortedMeshTerrains[i, j].x.ToString(), GUILayout.Width(50));
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    for (int j = 0; j < side; j++)
                    {
                        EditorGUILayout.TextField(sortedMeshTerrains[i, j].z.ToString(), GUILayout.Width(50));
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
            }
        }
        else
        {
            if (GUILayout.Button("Make all GOOD"))
            {
                FindNavMeshes();
                SortMeshTerrains();
                GenerateLinks();
            }
        }
        if (GUILayout.Button("Clear All Links"))
        {
            ClearAllNavmeshLinks();
        }
        showDebugInfo = (bool)EditorGUILayout.Toggle("showDebugInfo", showDebugInfo);
    }

    private void FindNavMeshes()
    {
        GameObject[] tmp = GameObject.FindObjectsOfType<GameObject>();
        List<GameObject> tmpList = new List<GameObject>();
        for (int i = 0; i < tmp.Length; i++)
            if (tmp[i].name.Contains(startName))
                tmpList.Add(tmp[i]);
        meshTerrains = tmpList.ToArray();
        totalCount = meshTerrains.Length;
        Debug.Log(totalCount + " mesh terrains found");
    }

    private void SelectSurfaces()
    {
        FindNavMeshes();
        List<GameObject> tmp = new List<GameObject>();
        for (int i = 0; i < meshTerrains.Length; i++)
        {
            NavMeshSurface nms = meshTerrains[i].GetComponent<NavMeshSurface>();
            if (nms == null)
                nms = meshTerrains[i].AddComponent<NavMeshSurface>();
            tmp.Add(nms.gameObject);
            Selection.objects = tmp.ToArray();
        }
    }

    private void SortMeshTerrains()
    {
        float tempSide = Mathf.Sqrt(meshTerrains.Length);
        side = (int)tempSide;
        if (tempSide - side > 0)
        {
            Debug.LogError("MeshTerrains not forming a square!");
            return;
        }

        // Fill temp array with sorted x values
        MeshTerrainSort[] tmpArray = new MeshTerrainSort[meshTerrains.Length];
        for (int i = 0; i < meshTerrains.Length; i++)
            tmpArray[i] = new MeshTerrainSort(meshTerrains[i]);

        for (int i = 0; i < meshTerrains.Length; i++)
            for (int j = 0; j < meshTerrains.Length - 1; j++)
                if (tmpArray[j].x > tmpArray[j + 1].x)
                {
                    MeshTerrainSort tmp = tmpArray[j];
                    tmpArray[j] = tmpArray[j + 1];
                    tmpArray[j + 1] = tmp;
                }

        // Fill array
        sortedMeshTerrains = new MeshTerrainSort[side, side];
        int tI = 0, tJ = 0;
        for (int i = 0; i < tmpArray.Length; i++)
        {
            sortedMeshTerrains[tI, tJ] = tmpArray[i];
            tI++;
            if (tI == side)
            {
                tI = 0;
                tJ++;
            }
        }

        Sort();
    }

    private class MeshTerrainSort
    {
        public GameObject meshTerrain = null;
        public float x = 0;
        public float z = 0;

        public MeshTerrainSort(GameObject _meshTerrain)
        {
            meshTerrain = _meshTerrain;
            x = _meshTerrain.transform.position.x;
            z = _meshTerrain.transform.position.z;
        }
    }

    private void Sort()
    {
        for (int i = 0; i < side; i++)
            Sort(i);
    }

    private void Sort(int _j)
    {
        for (int i = 0; i < side; i++)
            for (int j = 0; j < side - 1; j++)
                if (sortedMeshTerrains[j, _j].z < sortedMeshTerrains[j + 1, _j].z)
                    Swap(j, _j, j + 1, _j);
    }

    private void Swap(int fI, int fJ, int sI, int sJ)
    {
        MeshTerrainSort tmp = sortedMeshTerrains[sI, sJ];
        sortedMeshTerrains[sI, sJ] = sortedMeshTerrains[fI, fJ];
        sortedMeshTerrains[fI, fJ] = tmp;
    }

    private void GenerateLinks()
    {
        totalNavMeshLinks = 0;
        for (int i = 0; i < side; i++)
        {
            for (int j = 0; j < side; j++)
            {
                bool left = true, right = true, up = true, down = true;
                //if (i == 0)
                //    up = false;
                if (i == side - 1)
                    down = false;
                //if (j == 0)
                //    left = false;
                if (j == side - 1)
                    right = false;
                Debug.LogFormat("cell: {0}|{1}, right: {2}, down: {3}", i, j, right, down);
                //if (up)
                //    GenerateLink(sortedMeshTerrains[i, j].meshTerrain, Sides.up);
                if (down)
                    GenerateLink(sortedMeshTerrains[i, j].meshTerrain, Sides.down);
                //if (left)
                //    GenerateLink(sortedMeshTerrains[i, j].meshTerrain, Sides.left);
                if (right)
                    GenerateLink(sortedMeshTerrains[i, j].meshTerrain, Sides.right);
            }
        }
    }

    private enum Sides { left, right, up, down }
    private void GenerateLink(GameObject mt, Sides side)
    {
        float x, y, z;
        float startPointX, startPointZ, endPointX, endPointZ;
        float offset = navMeshLinkThickness / 2;
        switch (side)
        {
            case Sides.left:
                x = 0;
                z = meshWidth / 2;
                startPointX = x + offset;
                endPointX = x - offset;
                startPointZ = z;
                endPointZ = z;
                break;
            case Sides.right:
                x = meshWidth;
                z = meshWidth / 2;
                startPointX = x + offset;
                endPointX = x - offset;
                startPointZ = z;
                endPointZ = z;
                break;
            case Sides.up:
                x = meshWidth / 2;
                z = meshWidth;
                startPointX = x;
                endPointX = x;
                startPointZ = z + offset;
                endPointZ = z - offset;
                break;
            case Sides.down:
                x = meshWidth / 2;
                z = 0;
                startPointX = x;
                endPointX = x;
                startPointZ = z + offset;
                endPointZ = z - offset;
                break;
            default:
                Debug.LogError("No such Side");
                return;
        }

        MeshFilter mF = mt.GetComponent<MeshFilter>();
        if (mF == null)
            mF = mt.transform.GetChild(0).GetComponent<MeshFilter>();
        if (mF == null)
        {
            Debug.LogError("Cannot find mesh filter on mesh terrain");
            return;
        }
        Mesh mesh = mF.sharedMesh;
        float minDistanceSqr = Mathf.Infinity;
        Vector3 point = new Vector3(x, 0, z);
        Vector3 nearestVertex = Vector3.zero;
        foreach (Vector3 vertex in mesh.vertices)
        {
            Vector3 diff = point - vertex;
            float distSqr = diff.sqrMagnitude;
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                nearestVertex = vertex;
            }
        }
        y = nearestVertex.y;

        GameObject parent = null;
        Transform temp = mt.transform.Find("GENERATED_NAVMESHLINKS");
        if (temp != null)
            parent = temp.gameObject;

        if (parent == null)
        {
            parent = new GameObject("GENERATED_NAVMESHLINKS");
            parent.transform.parent = mt.transform;
            parent.transform.localPosition = Vector3.zero;
        }
        EditorSceneManager.MarkSceneDirty(parent.scene);

        NavMeshLink nmLink = parent.AddComponent<NavMeshLink>();
        nmLink.startPoint = new Vector3(startPointX, y, startPointZ);
        nmLink.endPoint = new Vector3(endPointX, y, endPointZ);
        nmLink.width = navMeshLinkWidth;
        nmLink.UpdateLink();
        totalNavMeshLinks++;
    }

    private void ClearAllNavmeshLinks()
    {
        GameObject[] AllGameObjects = GameObject.FindObjectsOfType<GameObject>();
        List<Transform> AllNavMeshLinks = new List<Transform>();
        for (int i = 0; i < AllGameObjects.Length; i++)
        {
            if (AllGameObjects[i].name.Contains("GENERATED_NAVMESHLINKS"))
                AllNavMeshLinks.Add(AllGameObjects[i].transform);
        }

        if (AllNavMeshLinks.Count == 0)
            Debug.Log("No generated navmesh links found");
        else
            for (int i = 0; i < AllNavMeshLinks.Count; i++)
            {
                EditorSceneManager.MarkSceneDirty(AllNavMeshLinks[i].gameObject.scene);
                DestroyImmediate(AllNavMeshLinks[i].gameObject);
            }
    }
}

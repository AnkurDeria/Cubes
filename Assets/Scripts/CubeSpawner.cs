using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CubeSpawner : MonoBehaviour
{
    [SerializeField] private int cubesPerSide;
    [SerializeField] private Transform cubesParent;
    [SerializeField] private Material cubeMat;
    [SerializeField] private float cubeGap;
    [SerializeField] private CinemachineVirtualCamera vCam;
    void Start()
    {
        cubeGap = cubeGap * Mathf.Pow(cubesPerSide,1/3f);
        float wallMid = (cubesPerSide + (cubesPerSide - 1) * cubeGap) / 2f;
        var distance =  wallMid*Mathf.Sqrt(2) / Mathf.Tan(vCam.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView * 0.5f * Mathf.Deg2Rad);
        vCam.transform.position = new Vector3(-distance/Mathf.Sqrt(2), wallMid,-distance / Mathf.Sqrt(2));
        transform.position = new Vector3(wallMid, wallMid, wallMid);
        //vCam.LookAt = cubesParent;
        List<Transform> cubeSides = new List<Transform>();
        cubeSides.Add(cubesParent.Find("Front"));
        cubeSides.Add(cubesParent.Find("Right"));
        cubeSides.Add(cubesParent.Find("Left"));
        cubeSides.Add(cubesParent.Find("Back"));
        float x, y, z;
        Vector3[] cubeVerts ={

            new Vector3(0f,0f,0f),
            new Vector3(1f,0f,0f),
            new Vector3(1f,1f,0f),
            new Vector3(0f,1f,0f),

            new Vector3(1f,0f,0f),
            new Vector3(1f,0f,1f),
            new Vector3(1f,1f,1f),
            new Vector3(1f,1f,0f),

            new Vector3(1f,0f,1f),
            new Vector3(0f,0f,1f),
            new Vector3(0f,1f,1f),
            new Vector3(1f,1f,1f),

            new Vector3(0f,0f,1f),
            new Vector3(0f,0f,0f),
            new Vector3(0f,1f,0f),
            new Vector3(0f,1f,1f),

            new Vector3(0f,1f,0f),
            new Vector3(1f,1f,0f),
            new Vector3(1f,1f,1f),
            new Vector3(0f,1f,1f),

            new Vector3(1f,0f,1f),
            new Vector3(1f,0f,0f),
            new Vector3(0f,0f,0f),
            new Vector3(0f,0f,1f)
        };
        Vector3[] cubeNormals = {

            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.left,
            -Vector3.left,
            -Vector3.left,
            -Vector3.left,
             Vector3.forward,
             Vector3.forward,
             Vector3.forward,
             Vector3.forward,
             Vector3.left,
             Vector3.left,
             Vector3.left,
             Vector3.left,
             Vector3.up,
             Vector3.up,
             Vector3.up,
             Vector3.up,
            -Vector3.up,
            -Vector3.up,
            -Vector3.up,
            -Vector3.up
        };
        int[] cubeTriangles = new int[36];
        for(int k=0;k<6;k++)
        {
            cubeTriangles[0 + 6 * k] = 4 * k;
            cubeTriangles[1 + 6 * k] = 4 * k + 2;
            cubeTriangles[2 + 6 * k] = 4 * k + 1;
            cubeTriangles[3 + 6 * k] = 4 * k;
            cubeTriangles[4 + 6 * k] = 4 * k + 3;
            cubeTriangles[5 + 6 * k] = 4 * k + 2;
        }
        List<GameObject> cubeParts = new List<GameObject>();
        
        Vector3[] normalBF = { -Vector3.forward, Vector3.forward };
        Vector3[] normalLR = { Vector3.left, -Vector3.left };
        Vector3[] normalBT = { -Vector3.up, Vector3.up };
        int parts = 0;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        MeshFilter mesh = null;
        for (int i=0;i<4;i++)
        {
            cubeParts.Add(new GameObject("Part" + parts.ToString()));
            cubeParts[parts].transform.parent = cubeSides[i];
            cubeParts[parts].AddComponent<MeshFilter>();
            cubeParts[parts].AddComponent<MeshRenderer>();
            cubeParts[parts].GetComponent<MeshRenderer>().material = cubeMat;
            mesh = cubeParts[parts].GetComponent<MeshFilter>();
            parts++;
            int xgap;
            int ygap;
            Vector3 origin = Vector3.zero;
            for (int j=0;j<cubesPerSide * cubesPerSide;j++)
            {
                xgap = j % cubesPerSide;
                ygap = j / cubesPerSide;

                origin = new Vector3(xgap + xgap * cubeGap, ygap + ygap * cubeGap, 0f);

                for (int k = 0; k < 24; k++)
                {
                    vertices.Add(origin + cubeVerts[k]);
                    normals.Add(cubeNormals[k]);
                }
                int vertStartIndex = vertices.Count - 24;
                for (int k = 0; k < 36; k++)
                {
                    triangles.Add(vertStartIndex + cubeTriangles[k]);
                }

                if(vertices.Count > 65519)
                {
                    mesh.mesh.SetVertices(vertices);
                    mesh.mesh.SetTriangles(triangles, 0);
                    mesh.mesh.SetNormals(normals);
                    //mesh.mesh.SetUVs(0, _uvs);
                    mesh.mesh.Optimize();
                    
                    cubeParts.Add(new GameObject("Part" + parts.ToString()));
                    cubeParts[parts].transform.parent = cubeSides[i];
                    cubeParts[parts].AddComponent<MeshFilter>();
                    cubeParts[parts].AddComponent<MeshRenderer>();
                    cubeParts[parts].GetComponent<MeshRenderer>().material = cubeMat;
                    mesh = cubeParts[parts].GetComponent<MeshFilter>();
                    parts++;
                    vertices.Clear();
                    triangles.Clear();
                    normals.Clear();
                }
               
            }
            mesh.mesh.SetVertices(vertices);
            mesh.mesh.SetTriangles(triangles, 0);
            mesh.mesh.SetNormals(normals);
            //mesh.mesh.SetUVs(0, _uvs);
            mesh.mesh.Optimize();
            vertices.Clear();
            triangles.Clear();
            normals.Clear();
        }
        if (mesh != null)
        {
            cubeSides[1].position += new Vector3(0f, 0f, cubesPerSide + (cubesPerSide - 1) * cubeGap);
            cubeSides[1].eulerAngles = new Vector3(0f, 90f, 0f);
            cubeSides[2].position += new Vector3(cubesPerSide - 1 + (cubesPerSide - 1) * cubeGap, 0f, cubesPerSide + (cubesPerSide - 1) * cubeGap);
            cubeSides[2].eulerAngles = new Vector3(0f, 90f, 0f);
            cubeSides[3].position = new Vector3(0f, 0f, cubesPerSide - 1 + (cubesPerSide - 1) * cubeGap);
        }
    }

    
    void Update()
    {
        
    }
}

using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;
using System;

public class CubeSpawner : MonoBehaviour
{
    [SerializeField] private TMP_InputField userInput;
    [SerializeField] private Transform cubesParent;
    [SerializeField] private Material cubeMat;
    [SerializeField] private float cubeGap;
    [SerializeField] private CinemachineVirtualCamera vCam;

    private Transform[] cubeSides = new Transform[4];
    private float wallMid;
    private float cubeToCamDist;
    private float actualCubeGap;
    private int cubesPerSide;
    private bool cameraMoveLeft = false;
    private bool cameraMoveRight = false;
    private Vector3[] cubeVerts ={

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
    private Vector3[] cubeNormals = {

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
    private int[] cubeTriangles = new int[36];
    private List<GameObject> cubeParts = new List<GameObject>();
    private int parts = 0;
    private List<Vector3> vertices = new List<Vector3>();
    private List<Color32> colours = new List<Color32>();
    private List<int> triangles = new List<int>();
    private List<Vector3> normals = new List<Vector3>();
    private MeshFilter mesh = null;
    private Vector3 origin = Vector3.zero;
    private int xOffset;
    private int yOffset;
    private int vertStartIndex;

    private void Awake()
    {
        cubeSides[0] = cubesParent.Find("Front");
        cubeSides[1] = cubesParent.Find("Right");
        cubeSides[2] = cubesParent.Find("Left");
        cubeSides[3] = cubesParent.Find("Back");
        for (int k = 0; k < 6; k++)
        {
            cubeTriangles[0 + 6 * k] = 4 * k;
            cubeTriangles[1 + 6 * k] = 4 * k + 2;
            cubeTriangles[2 + 6 * k] = 4 * k + 1;
            cubeTriangles[3 + 6 * k] = 4 * k;
            cubeTriangles[4 + 6 * k] = 4 * k + 3;
            cubeTriangles[5 + 6 * k] = 4 * k + 2;
        }
    }

    public void CubeGen()
    {
        if (userInput.text == "" || userInput.text[0] == '-')
        {
            userInput.text = "";
            return;
        }
        if (!int.TryParse(userInput.text, out cubesPerSide))
        {
            userInput.text = "";
            return;
        }
        if (cubesPerSide == 0)
        {
            return;
        }
        ClearVariables();
        actualCubeGap = cubeGap * Mathf.Pow(cubesPerSide, 1f / 3f);
        wallMid = (cubesPerSide + (cubesPerSide - 1) * actualCubeGap) / 2f;
        cubeToCamDist = wallMid * Mathf.Sqrt(2) / Mathf.Tan(vCam.m_Lens.FieldOfView * 0.5f * Mathf.Deg2Rad);
        vCam.transform.position = new Vector3(-cubeToCamDist / 2, wallMid, -cubeToCamDist / 2);
        
        vCam.m_Lens.FarClipPlane = Mathf.Clamp(2 * cubeToCamDist + cubeToCamDist / Mathf.Sqrt(2),50,Mathf.Infinity);
        transform.position = new Vector3(wallMid, wallMid, wallMid);


        for (int i = 0; i < 4; i++)
        {
            cubeParts.Add(new GameObject("Part" + parts.ToString()));
            cubeParts[parts].transform.parent = cubeSides[i];
            cubeParts[parts].AddComponent<MeshFilter>();
            cubeParts[parts].AddComponent<MeshRenderer>();
            cubeParts[parts].GetComponent<MeshRenderer>().material = cubeMat;
            
            mesh = cubeParts[parts].GetComponent<MeshFilter>();
            parts++;

            for (int j = 0; j < cubesPerSide * cubesPerSide; j++)
            {
                xOffset = j % cubesPerSide;
                yOffset = j / cubesPerSide;
                
                origin = new Vector3(xOffset + xOffset * actualCubeGap, yOffset + yOffset * actualCubeGap, 0f);
                byte rand1 = (byte)UnityEngine.Random.Range(0, 256);
                byte rand2 = (byte)UnityEngine.Random.Range(0, 256);
                byte rand3 = (byte)UnityEngine.Random.Range(0, 256);
                for (int k = 0; k < 24; k++)
                {
                    vertices.Add(origin + cubeVerts[k]);
                    colours.Add(new Color32(rand1,rand2,rand3,255));
                    normals.Add(cubeNormals[k]);
                }
                vertStartIndex = vertices.Count - 24;
                for (int k = 0; k < 36; k++)
                {
                    triangles.Add(vertStartIndex + cubeTriangles[k]);
                }

                if (vertices.Count > 65519)
                {
                    mesh.mesh.SetVertices(vertices);
                    mesh.mesh.SetTriangles(triangles, 0);
                    mesh.mesh.SetNormals(normals);
                    mesh.mesh.SetColors(colours);
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
                    colours.Clear();
                }

            }
            mesh.mesh.SetVertices(vertices);
            mesh.mesh.SetTriangles(triangles, 0);
            mesh.mesh.SetNormals(normals);
            mesh.mesh.SetColors(colours);
            //mesh.mesh.SetUVs(0, _uvs);
            mesh.mesh.Optimize();
            vertices.Clear();
            triangles.Clear();
            normals.Clear();
            colours.Clear();
        }
        if (mesh != null)
        {
            cubeSides[1].position += new Vector3(0f, 0f, cubesPerSide + (cubesPerSide - 1) * actualCubeGap);
            cubeSides[1].eulerAngles = new Vector3(0f, 90f, 0f);
            cubeSides[2].position += new Vector3(cubesPerSide - 1 + (cubesPerSide - 1) * actualCubeGap, 0f, cubesPerSide + (cubesPerSide - 1) * actualCubeGap);
            cubeSides[2].eulerAngles = new Vector3(0f, 90f, 0f);
            cubeSides[3].position = new Vector3(0f, 0f, cubesPerSide - 1 + (cubesPerSide - 1) * actualCubeGap);
        }

    }
    public void LeftButton()
    {
        cameraMoveLeft = !cameraMoveLeft;


    }
    public void RightButton()
    {
        cameraMoveRight = !cameraMoveRight;


    }
    private void Update()
    {
        if (!(cameraMoveLeft == cameraMoveRight))
        {
            if (cameraMoveRight)
            {
                vCam.transform.RotateAround(transform.position, Vector3.up, -10 * Time.deltaTime);
            }
            else
            {
                vCam.transform.RotateAround(transform.position, Vector3.up, 10 * Time.deltaTime);
            }
        }
    }
    private void ClearVariables()
    {
        parts = 0;
        if (cubeParts.Count != 0)
        {
            for (int i = 0; i < cubeParts.Count; i++)
            {
                GameObject.Destroy(cubeParts[i]);
            }
        }
        cubeParts.Clear();
        for (int i = 0; i < 4; i++)
        {
            cubeSides[i].position = Vector3.zero;
            cubeSides[i].eulerAngles = Vector3.zero;
        }
    }
}

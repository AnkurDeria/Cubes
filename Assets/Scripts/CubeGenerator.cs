using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;


public class CubeGenerator : MonoBehaviour
{
    [SerializeField] private TMP_InputField userInput;
    [SerializeField] private Transform cubesParent;
    [SerializeField] private Material cubeMat;
    [SerializeField] private float cubeGap;
    [SerializeField] private CinemachineVirtualCamera vCam;

    private Transform[] walls = new Transform[4];
    
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
    private MeshFilter meshFilter = null;
    private Vector3 origin = Vector3.zero;
    private int xOffset;
    private int yOffset;
    private int vertStartIndex;
    private const float SQRT_TWO = 1.414f;
    private void Awake()
    {
        Application.targetFrameRate = 60;
        walls[0] = cubesParent.Find("BWall1");
        walls[1] = cubesParent.Find("SWall1");
        walls[2] = cubesParent.Find("BWall2");
        walls[3] = cubesParent.Find("SWall2");
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

        ClearVariables();

        if (cubesPerSide == 0)
        {
            return;
        }
        
        actualCubeGap = cubeGap * Mathf.Pow(cubesPerSide, 1f / 3f);
        SetCameraPosition();

        for (int i = 0; i < 2; i++)
        {
            WallGen(i);

            if (cubesPerSide > 1)
            {
                if (i == 0)
                {
                    walls[i+2].position = walls[i].position + new Vector3(0f, 0f, cubesPerSide - 1 + (cubesPerSide - 1) * actualCubeGap);
                }
                else if(cubesPerSide > 2)
                {
                    walls[i].position += new Vector3(1f + actualCubeGap, 0f, 0f);
                    walls[i + 2].position += new Vector3(1f + actualCubeGap, 0f, 0f);
                    walls[i].RotateAround(transform.position, Vector3.up, 90f);

                    walls[i + 2].RotateAround(transform.position, Vector3.up, -90f);
                }
                else
                {
                    break;
                }
                
            }
            else
            {
                break;
            }
        }
        StaticBatchingUtility.Combine(cubeParts.ToArray(),cubesParent.gameObject);
    }

    private void SetCameraPosition()
    {
        wallMid = (cubesPerSide + (cubesPerSide - 1) * actualCubeGap) / 2f;
        cubeToCamDist = wallMid * SQRT_TWO / Mathf.Tan(vCam.m_Lens.FieldOfView * 0.5f * Mathf.Deg2Rad);
        vCam.transform.position = new Vector3(-cubeToCamDist / 2, wallMid, -cubeToCamDist / 2);

        vCam.m_Lens.FarClipPlane = Mathf.Clamp(2 * cubeToCamDist + cubeToCamDist / SQRT_TWO, 50, Mathf.Infinity);
        transform.position = new Vector3(wallMid, wallMid, wallMid);
    }

    private void WallGen( int _i)
    {
        cubeParts.Add(new GameObject("Part" + parts.ToString()));
        cubeParts[parts].transform.parent = walls[_i];
        cubeParts[parts].isStatic = true;
        cubeParts[parts].AddComponent<MeshFilter>();
        cubeParts[parts].AddComponent<MeshRenderer>();
        cubeParts[parts].GetComponent<MeshRenderer>().sharedMaterial = cubeMat;

        meshFilter = cubeParts[parts].GetComponent<MeshFilter>();
        parts++;
       
        for (int j = 0; j < cubesPerSide * (cubesPerSide - 2*_i); j++)
        {
            xOffset = j % (cubesPerSide - 2 * _i);
            yOffset = j / (cubesPerSide - 2 * _i);
           
            origin = new Vector3(xOffset + xOffset * actualCubeGap, yOffset + yOffset * actualCubeGap, 0f);
            byte rand1 = (byte)UnityEngine.Random.Range(0, 256);
            byte rand2 = (byte)UnityEngine.Random.Range(0, 256);
            byte rand3 = (byte)UnityEngine.Random.Range(0, 256);
            for (int k = 0; k < 24; k++)
            {
                vertices.Add(origin + cubeVerts[k]);
                colours.Add(new Color32(rand1, rand2, rand3, 255));
                normals.Add(cubeNormals[k]);
            }
            vertStartIndex = vertices.Count - 24;
            for (int k = 0; k < 36; k++)
            {
                triangles.Add(vertStartIndex + cubeTriangles[k]);
            }
        }
        meshFilter.mesh.SetVertices(vertices);
        meshFilter.mesh.SetTriangles(triangles, 0);
        meshFilter.mesh.SetNormals(normals);
        meshFilter.mesh.SetColors(colours);
        //mesh.mesh.SetUVs(0, _uvs);
        meshFilter.mesh.Optimize();
        if (cubesPerSide > 1)
        {
            Instantiate(cubeParts[parts - 1], Vector3.zero, Quaternion.identity, walls[_i + 2]);
        }
        vertices.Clear();
        triangles.Clear();
        normals.Clear();
        colours.Clear();
        
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
    public void ClearVariables()
    {
        parts = 0;
       
        
        cubeParts.Clear();
        for (int i = 0; i < 4; i++)
        {
            foreach(Transform child in walls[i])
            {
                GameObject.Destroy(child.gameObject);
            }
            walls[i].position = Vector3.zero;
            walls[i].eulerAngles = Vector3.zero;
        }
    }
}

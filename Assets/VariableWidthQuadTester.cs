using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class VariableWidthQuadTester : MonoBehaviour
{
    public enum CalculationType
    {
        SimpleLength,
        ProjectedLength
    }
    
    private MeshRenderer m_Renderer;
    private MeshFilter m_MeshFilter;

    private Mesh m_Mesh;

    private bool m_IsStarted = false;

    private bool m_RefreshRequired = false;
    
    [Header("Side sizes")]

    [SerializeField]
    [Range(0.0f, 2.0f)]
    private float m_LeftSize = 1.0f;

    [SerializeField]
    [Range(0.0f, 2.0f)]
    private float m_RightSize = 1.0f;
    
    [Header("W")]

    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float m_TopLeftW = 1.0f;

    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float m_BottomLeftW = 1.0f;
    

    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float m_TopRightW = 1.0f;

    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float m_BottomRightW = 1.0f;
    
    
    [Header("Auto W calculation")]

    [SerializeField]
    private bool m_AutoCalculationOfW = false;

    [SerializeField]
    private CalculationType m_CalculationType = CalculationType.SimpleLength;
    
    [Header("Horizontal Shift")]
    
    [SerializeField]
    [Range(-1.0f, 1.0f)]
    private float m_TopLeftShiftX = 0.0f;

    [SerializeField]
    [Range(-1.0f, 1.0f)]
    private float m_TopRightShiftX = 0.0f;

    [SerializeField]
    [Range(-1.0f, 1.0f)]
    private float m_BottomRightShiftX = 0.0f;

    [SerializeField]
    [Range(-1.0f, 1.0f)]
    private float m_BottomLeftShiftX = 0.0f;
    
    
    
    void Awake()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        m_MeshFilter = GetComponent<MeshFilter>();

        m_Mesh = new Mesh();
        m_Mesh.MarkDynamic();
        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        m_IsStarted = true;
        m_RefreshRequired = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_RefreshRequired)
        {
            SetupMesh();
        }
    }


    private void OnValidate()
    {
        m_RefreshRequired = true;
    }


    private static int[] s_Indices = {0, 1, 2, 0, 2, 3};
    
    private void SetupMesh()
    {
        if (m_IsStarted == false) return;
        
        //vertices
        List<Vector3> vertices = new List<Vector3>()
        {
            new Vector3(-1.0f + m_TopLeftShiftX, 1.0f * m_LeftSize, 0.0f),
            new Vector3(1.0f + m_TopRightShiftX, 1.0f * m_RightSize, 0.0f),
            new Vector3(1.0f + m_BottomRightShiftX, -1.0f * m_RightSize, 0.0f),
            new Vector3(-1.0f + m_BottomLeftShiftX, -1.0f * m_LeftSize, 0.0f)
        };

        
        //uv0
        List<Vector2> uv0 = new List<Vector2>()
        {
            new Vector2(0.0f, 1.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 0.0f)
        };

        //uv1 (u is for w)
        List<Vector2> uv1 = new List<Vector2>()
        {
            new Vector2(m_TopLeftW, 0.0f),
            new Vector2(m_TopRightW, 0.0f),
            new Vector2(m_BottomRightW, 0.0f),
            new Vector2(m_BottomLeftW, 0.0f)
        };
        
        //auto calculation of W
        if (m_AutoCalculationOfW)
        {
            if (m_CalculationType == CalculationType.SimpleLength)
            {
                float leftLength = (vertices[0] - vertices[3]).magnitude;
                float rightLength = (vertices[1] - vertices[2]).magnitude;

                float maxLength = Mathf.Max(leftLength, rightLength);

                float leftW = maxLength / leftLength;
                float rightW = maxLength / rightLength;

                uv1[0] = new Vector2(leftW, 0.0f);
                uv1[1] = new Vector2(rightW, 0.0f);
                uv1[2] = new Vector2(rightW, 0.0f);
                uv1[3] = new Vector2(leftW, 0.0f);
            }
            else if (m_CalculationType == CalculationType.ProjectedLength)
            {
                Vector2 leftCenter = Vector3.Lerp(vertices[0], vertices[3], 0.5f);
                Vector2 rightCenter = Vector3.Lerp(vertices[1], vertices[2], 0.5f);

                Vector2 fromLeftToRight = rightCenter - leftCenter;
                Vector2 fromLeftToRightNormal = fromLeftToRight.GetNormal();

                Vector2 fromTopLeftToBottomLeft = vertices[3] - vertices[0];
                Vector2 fromTopRightToBottomRight = vertices[2] - vertices[1];

                float leftProjectedLength = Vector2.Dot(fromTopLeftToBottomLeft, fromLeftToRightNormal);
                float rightProjectedLength = Vector2.Dot(fromTopRightToBottomRight, fromLeftToRightNormal);

                float maxLength = Mathf.Max(leftProjectedLength, rightProjectedLength);
                
                float leftW = maxLength / leftProjectedLength;
                float rightW = maxLength / rightProjectedLength;

                uv1[0] = new Vector2(leftW, 0.0f);
                uv1[1] = new Vector2(rightW, 0.0f);
                uv1[2] = new Vector2(rightW, 0.0f);
                uv1[3] = new Vector2(leftW, 0.0f);
                
            }
            else
            {
                Debug.LogWarning("Unhandled calculation type: " + m_CalculationType);
            }
        }
        
        
        //-----
        //applying to mesh
        m_Mesh.SetVertices(vertices);
        m_Mesh.SetUVs(0, uv0);
        m_Mesh.SetUVs(1, uv1);
        
        m_Mesh.SetIndices(s_Indices, MeshTopology.Triangles, 0);
        m_Mesh.RecalculateBounds();

        //-----
        //setting mesh to filter
        m_MeshFilter.sharedMesh = m_Mesh;

    }
    
    
}

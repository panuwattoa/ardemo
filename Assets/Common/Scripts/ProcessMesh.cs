using System;
using System.Collections;
using System.Collections.Generic;
using JoystickLab;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class ProcessMesh : MonoBehaviour
{
   private Mesh m_mesh;
   private MeshRenderer m_renderer;
   private Vector3[] m_vertices;
   private Vector2[] m_tringles;
   private int[] indices;
   private float scale = 2;
   private void Awake()
   {
      m_mesh = GetComponent<MeshFilter>().mesh;
      m_renderer = GetComponent<MeshRenderer>();
   }

   private void Update()
   {
      if (LineRendererDrawing.Instance.pointStack.Count >= 2)
      {
         MakeMeshData();
         CreateMesh();
      }
   }

   void MakeMeshData()
   {
      var listPoint = new List<Vector3>();
      foreach (var line in LineRendererDrawing.Instance.pointStack)
      {
         listPoint.Add(line.point.transform.position);
      }
      m_vertices = new Vector3[]{};
      m_vertices = listPoint.ToArray();
      m_tringles = new Vector2[] { };
      var tringlesPoint = new List<Vector2>();
      for (int i = 0; i < m_vertices.Length; i++)
      {
         tringlesPoint.Add(new Vector2(m_vertices[i].x, m_vertices[i].y));
      }

      m_tringles = tringlesPoint.ToArray();
      // Use the triangulator to get indices for creating triangles
       Triangulator tr = new Triangulator(m_tringles);
       indices = tr.Triangulate();
   }

   void CreateMesh()
   {
      m_mesh.Clear();
      m_mesh.vertices = m_vertices;

      m_mesh.triangles = indices;
      // Debug.Log("m_tringles count  " + m_tringles.Length);

      m_mesh.RecalculateNormals();
      m_mesh.RecalculateBounds();

   }

   public void ScaleSize()
   {
      scale = scale * 0.5f + 1;
      m_renderer.material.SetTextureScale("ThreeMat",new Vector2(scale, scale));
   }
   public void ScaleSizeDown()
   {
       scale = scale * 0.5f - 1;
      // float scaleY = scale * 0.5f - 1;
      m_renderer.material.SetTextureScale("ThreeMat",new Vector2(scale, scale));
   }
}

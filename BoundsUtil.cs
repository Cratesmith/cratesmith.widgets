using System;
using System.Collections.Generic;
using UnityEngine;
namespace SubModules.com.cratesmith.widgets
{
    public static class BoundsUtil
    {
        static List<Renderer> s_RendererList = new List<Renderer>();

        public static Vector3 GetCorner(in Bounds bounds, int index, Transform boundsRelativeTo = null)
        {
            Vector3 result;
            switch (index)
            {
                case 0:  result = new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z) + bounds.center; break; 
                case 1:  result = new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z) + bounds.center; break;
                case 2:  result = new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z) + bounds.center; break;
                case 3:  result = new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z) + bounds.center; break;
                case 4:  result = new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z) + bounds.center; break;
                case 5:  result = new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z) + bounds.center; break;
                case 6:  result = new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z) + bounds.center; break;
                case 7:  result = new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z) + bounds.center; break;
                default: throw new ArgumentOutOfRangeException();
            }

            return boundsRelativeTo
                ? boundsRelativeTo.TransformPoint(result)
                : result;
        }
        
        
        public static Rect GetScreenRect(Bounds bounds, Camera camera, Transform relativeTo=null)
        {
            if (camera == null)
            {
                throw new ArgumentNullException("camera");
            }

            var r = new Rect();

            for (int i = 0; i < 8; i++)
            {
                var pos = GetCorner(bounds,i, relativeTo);
                var screenPos = camera.WorldToScreenPoint(pos );
                if (i==0)
                {
                    r = new Rect(screenPos, Vector2.zero);
                } else
                {
                    r.xMin = Mathf.Min(r.xMin, screenPos.x);
                    r.yMin = Mathf.Min(r.yMin, screenPos.y);
                    r.xMax = Mathf.Max(r.xMax, screenPos.x);
                    r.yMax = Mathf.Max(r.yMax, screenPos.y);
                }
            }
            return r;
        }
        
        public static Bounds GetLocalMeshRenderBounds(Transform transform, int layerMask= -1)
        {
            Bounds output = new Bounds(Vector3.zero, Vector3.zero);
            transform.GetComponentsInChildren(s_RendererList);
            foreach (var renderer in s_RendererList)
            {
                if ((1 << renderer.gameObject.layer & layerMask) == 0)
                {
                    continue;
                }

                if (renderer.enabled == false)
                {
                    continue;
                }

                Bounds renderBounds; 
                switch (renderer)
                {
                    case MeshRenderer mr:
                        var mf = mr.GetComponent<MeshFilter>();
                        renderBounds = mf.sharedMesh.bounds;
                        break;
                    case SkinnedMeshRenderer smr:
                        renderBounds = smr.localBounds;
                        break;
                    default:
                        continue;
                }
                

                if (renderer.bounds.size == Vector3.zero)
                {
                    continue;
                }

                for (int i = 0; i < 8; i++)
                {
                    var point = transform.InverseTransformPoint(GetCorner(renderBounds, i, renderer.transform));

                    if (output.center == Vector3.zero && output.size == Vector3.zero)
                        output = new Bounds(point, Vector3.zero);
                    else
                        output.Encapsulate(point);
                }
            }
            s_RendererList.Clear();
            return output;
        }
        
        public static void DrawGizmo(Bounds bounds, Transform transform=null)
        {
            var pop = Gizmos.matrix;
            if (transform!=null)
            {
                Gizmos.matrix *= transform.localToWorldMatrix;			
            }
            Gizmos.DrawWireCube(bounds.center, bounds.extents*2);
            Gizmos.matrix = pop;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CleanToContinue.Surface
{
    internal sealed class TriangleSurfaceCleaner : IDisposable
    {
        private readonly MeshFilter targetFilter;
        private readonly Mesh sourceMesh;
        private readonly Mesh runtimeMesh;
        private readonly int[][] sourceIndices;
        private readonly bool[][] remainingTriangles;
        private readonly int totalTriangleCount;
        private int remainingTriangleCount;

        public TriangleSurfaceCleaner(MeshFilter filter)
        {
            targetFilter = filter != null ? filter : throw new ArgumentNullException(nameof(filter));
            sourceMesh = filter.sharedMesh != null
                ? filter.sharedMesh
                : throw new ArgumentException("The triangle cleaner requires a mesh.", nameof(filter));
            runtimeMesh = UnityEngine.Object.Instantiate(sourceMesh);
            runtimeMesh.name = $"{sourceMesh.name} (Runtime Cleaning Mask)";
            runtimeMesh.hideFlags = HideFlags.HideAndDontSave;

            var vertices = runtimeMesh.vertices;
            sourceIndices = new int[runtimeMesh.subMeshCount][];
            remainingTriangles = new bool[runtimeMesh.subMeshCount][];
            for (var subMesh = 0; subMesh < runtimeMesh.subMeshCount; subMesh++)
            {
                sourceIndices[subMesh] = runtimeMesh.GetIndices(subMesh);
                var triangleCount = sourceIndices[subMesh].Length / 3;
                remainingTriangles[subMesh] = new bool[triangleCount];
                for (var triangle = 0; triangle < triangleCount; triangle++)
                {
                    var index = triangle * 3;
                    var first = vertices[sourceIndices[subMesh][index]];
                    var second = vertices[sourceIndices[subMesh][index + 1]];
                    var third = vertices[sourceIndices[subMesh][index + 2]];
                    if (Vector3.Cross(second - first, third - first).sqrMagnitude <= 0.0000000001f)
                    {
                        continue;
                    }

                    remainingTriangles[subMesh][triangle] = true;
                    totalTriangleCount++;
                }
            }

            remainingTriangleCount = totalTriangleCount;
            targetFilter.sharedMesh = runtimeMesh;
        }

        public float Progress01 => totalTriangleCount == 0
            ? 1f
            : 1f - remainingTriangleCount / (float)totalTriangleCount;

        public int Clean(Vector3 worldPoint, float normalizedRadius)
        {
            if (normalizedRadius <= 0f || remainingTriangleCount == 0)
            {
                return 0;
            }

            var localPoint = targetFilter.transform.InverseTransformPoint(worldPoint);
            var boundsSize = sourceMesh.bounds.size;
            var localRadius = normalizedRadius * Mathf.Max(boundsSize.x, boundsSize.y, boundsSize.z);
            var radiusSquared = localRadius * localRadius;
            var vertices = sourceMesh.vertices;
            var newlyCleaned = 0;

            for (var subMesh = 0; subMesh < sourceIndices.Length; subMesh++)
            {
                var indices = sourceIndices[subMesh];
                var remaining = remainingTriangles[subMesh];
                for (var triangle = 0; triangle < remaining.Length; triangle++)
                {
                    if (!remaining[triangle])
                    {
                        continue;
                    }

                    var index = triangle * 3;
                    var closest = ClosestPointOnTriangle(
                        localPoint,
                        vertices[indices[index]],
                        vertices[indices[index + 1]],
                        vertices[indices[index + 2]]);
                    if ((closest - localPoint).sqrMagnitude > radiusSquared)
                    {
                        continue;
                    }

                    remaining[triangle] = false;
                    remainingTriangleCount--;
                    newlyCleaned++;
                }
            }

            if (newlyCleaned > 0)
            {
                RebuildVisibleTriangles();
            }

            return newlyCleaned;
        }

        public void ForceFinish()
        {
            if (remainingTriangleCount == 0)
            {
                return;
            }

            foreach (var remaining in remainingTriangles)
            {
                Array.Fill(remaining, false);
            }

            remainingTriangleCount = 0;
            RebuildVisibleTriangles();
        }

        public void Dispose()
        {
            if (targetFilter != null && targetFilter.sharedMesh == runtimeMesh)
            {
                targetFilter.sharedMesh = sourceMesh;
            }

            if (runtimeMesh != null)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(runtimeMesh);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(runtimeMesh);
                }
            }
        }

        private void RebuildVisibleTriangles()
        {
            for (var subMesh = 0; subMesh < sourceIndices.Length; subMesh++)
            {
                var source = sourceIndices[subMesh];
                var remaining = remainingTriangles[subMesh];
                var visible = new List<int>(source.Length);
                for (var triangle = 0; triangle < remaining.Length; triangle++)
                {
                    if (!remaining[triangle])
                    {
                        continue;
                    }

                    var index = triangle * 3;
                    visible.Add(source[index]);
                    visible.Add(source[index + 1]);
                    visible.Add(source[index + 2]);
                }

                runtimeMesh.SetIndices(visible, MeshTopology.Triangles, subMesh, false);
            }

            runtimeMesh.RecalculateBounds();
        }

        private static Vector3 ClosestPointOnTriangle(Vector3 point, Vector3 first, Vector3 second, Vector3 third)
        {
            var firstSecond = second - first;
            var firstThird = third - first;
            var firstPoint = point - first;
            var firstProjection = Vector3.Dot(firstSecond, firstPoint);
            var secondProjection = Vector3.Dot(firstThird, firstPoint);
            if (firstProjection <= 0f && secondProjection <= 0f)
            {
                return first;
            }

            var secondPoint = point - second;
            var thirdProjection = Vector3.Dot(firstSecond, secondPoint);
            var fourthProjection = Vector3.Dot(firstThird, secondPoint);
            if (thirdProjection >= 0f && fourthProjection <= thirdProjection)
            {
                return second;
            }

            var firstEdge = firstProjection * fourthProjection - thirdProjection * secondProjection;
            if (firstEdge <= 0f && firstProjection >= 0f && thirdProjection <= 0f)
            {
                return first + firstSecond * (firstProjection / (firstProjection - thirdProjection));
            }

            var thirdPoint = point - third;
            var fifthProjection = Vector3.Dot(firstSecond, thirdPoint);
            var sixthProjection = Vector3.Dot(firstThird, thirdPoint);
            if (sixthProjection >= 0f && fifthProjection <= sixthProjection)
            {
                return third;
            }

            var secondEdge = fifthProjection * secondProjection - firstProjection * sixthProjection;
            if (secondEdge <= 0f && secondProjection >= 0f && sixthProjection <= 0f)
            {
                return first + firstThird * (secondProjection / (secondProjection - sixthProjection));
            }

            var thirdEdge = thirdProjection * sixthProjection - fifthProjection * fourthProjection;
            if (thirdEdge <= 0f && fourthProjection - thirdProjection >= 0f && fifthProjection - sixthProjection >= 0f)
            {
                return second + (third - second) * ((fourthProjection - thirdProjection) /
                    ((fourthProjection - thirdProjection) + (fifthProjection - sixthProjection)));
            }

            var denominator = 1f / (firstEdge + secondEdge + thirdEdge);
            var secondWeight = secondEdge * denominator;
            var thirdWeight = thirdEdge * denominator;
            return first + firstSecond * secondWeight + firstThird * thirdWeight;
        }
    }
}

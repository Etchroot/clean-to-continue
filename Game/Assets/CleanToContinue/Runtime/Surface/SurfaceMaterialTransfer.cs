using System;
using UnityEngine;

namespace CleanToContinue.Surface
{
    public static class SurfaceMaterialTransfer
    {
        public static void CopyToCleanable(Material source, Material target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            CopyTexture(source, target, "_BaseMap", "_BaseMap", "_MainTex");
            CopyColor(source, target, "_BaseColor", "_BaseColor", "_Color");
            CopyTexture(source, target, "_BumpMap", "_BumpMap");
            CopyFloat(source, target, "_BumpScale", "_BumpScale");
            CopyTexture(source, target, "_MetallicGlossMap", "_MetallicGlossMap");
            CopyFloat(source, target, "_Metallic", "_Metallic");
            CopyFloat(source, target, "_CleanSmoothness", "_Smoothness", "_Glossiness");
        }

        private static void CopyTexture(
            Material source,
            Material target,
            string targetProperty,
            params string[] sourceProperties)
        {
            if (!target.HasProperty(targetProperty))
            {
                return;
            }

            foreach (var sourceProperty in sourceProperties)
            {
                if (!source.HasProperty(sourceProperty))
                {
                    continue;
                }

                target.SetTexture(targetProperty, source.GetTexture(sourceProperty));
                target.SetTextureScale(targetProperty, source.GetTextureScale(sourceProperty));
                target.SetTextureOffset(targetProperty, source.GetTextureOffset(sourceProperty));
                return;
            }
        }

        private static void CopyColor(
            Material source,
            Material target,
            string targetProperty,
            params string[] sourceProperties)
        {
            if (!target.HasProperty(targetProperty))
            {
                return;
            }

            foreach (var sourceProperty in sourceProperties)
            {
                if (source.HasProperty(sourceProperty))
                {
                    target.SetColor(targetProperty, source.GetColor(sourceProperty));
                    return;
                }
            }
        }

        private static void CopyFloat(
            Material source,
            Material target,
            string targetProperty,
            params string[] sourceProperties)
        {
            if (!target.HasProperty(targetProperty))
            {
                return;
            }

            foreach (var sourceProperty in sourceProperties)
            {
                if (source.HasProperty(sourceProperty))
                {
                    target.SetFloat(targetProperty, source.GetFloat(sourceProperty));
                    return;
                }
            }
        }
    }
}

using System;
using Unity.Mathematics;
using UnityEngine;

namespace TerrainTopology
{

    [System.Serializable]
    public class CreateAspectMap : CreateTopology
    {

        protected override bool OnChange()
        {
            return m_currentColorMode != m_coloredGradient;
        }

        public override Color[] CreateMap()
        {

            Color[] map = new Color[m_width * m_height];

            for (int y = 0; y < m_height; y++)
            {
                for (int x = 0; x < m_width; x++)
                {
                    float2 d1 = GetFirstDerivative(x, y);

                    float aspect = (float)Aspect(d1.x, d1.y);

                    map[x + y * m_width] = Colorize(aspect, 0, true);
                }

            }
            return map;
        }

        private float Aspect(float zx, float zy)
        {
            float gyx = FMath.SafeDiv(zy, zx);
            float gxx = FMath.SafeDiv(zx, Math.Abs(zx));

            float aspect = 180 - math.atan(gyx) * FMath.Rad2Deg + 90 * gxx;
            aspect /= 360;

            return aspect;
        }

    }

}

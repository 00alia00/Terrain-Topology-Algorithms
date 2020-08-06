using Unity.Mathematics;
using UnityEngine;

namespace TerrainTopology
{

    [System.Serializable]
    public class CreateSlopeMap : CreateTopology
    {
        protected override bool OnChange()
        {
            return m_currentColorMode != m_coloredGradient;
        }

        public override Color[] CreateMap()
        {
            Color[] map = new Color[m_width*m_height];
            for (int y = 0; y < m_height; y++)
            {
                for (int x = 0; x < m_width; x++)
                {
                    float2 d1 = GetFirstDerivative(x, y);

                    float slope = Slope(d1.x, d1.y);

                    map[x+y*m_width] =  Colorize(slope, 0.4f, true);
                }
            }

            return map;
        }

        private float Slope(float zx, float zy)
        {
            float p = zx * zx + zy * zy;
            float g = FMath.SafeSqrt(p);

            return math.atan(g) * FMath.Rad2Deg / 90.0f;
        }
    }
}
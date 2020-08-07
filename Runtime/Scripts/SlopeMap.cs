using Unity.Mathematics;

namespace TerrainTopology
{
    public class SlopeMap : Topology
    {
        protected override bool OnChange()
        {
            return m_currentColorMode != m_coloredGradient;
        }

        public override float4[] CreateMap()
        {
            float4[] map = new float4[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float2 d1 = GetFirstDerivative(x, y);

                    float slope = Slope(d1.x, d1.y);

                    map[x + y * width] = Colorize(slope, 0.4f, true);
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
using System;
using Unity.Mathematics;

namespace TerrainTopology
{
    public class CreateAspectMap : CreateTopology
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

                    float aspect = (float)Aspect(d1.x, d1.y);

                    map[x + y * width] = Colorize(aspect, 0, true);
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

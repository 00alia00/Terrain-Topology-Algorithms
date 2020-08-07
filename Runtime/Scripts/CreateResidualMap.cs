using System.Collections.Generic;
using Unity.Mathematics;

namespace TerrainTopology
{
    public enum RESIDUAL_TYPE { ELEVATION, MEAN, DIFFERENCE, STDEV, DEVIATION, PERCENTILE };

    public class CreateResidualMap : CreateTopology
    {

        public RESIDUAL_TYPE m_residualType = RESIDUAL_TYPE.PERCENTILE;

        private int m_window = 3;

        private RESIDUAL_TYPE m_currentType;

        protected override bool OnChange()
        {
            return m_currentType != m_residualType || m_currentColorMode != m_coloredGradient;
        }

        public override float4[] CreateMap()
        {
            m_currentType = m_residualType;

            float4[] map = new float4[width * height];

            var elevations = new List<float>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    elevations.Clear();

                    for (int i = -m_window; i <= m_window; i++)
                    {
                        for (int j = -m_window; j <= m_window; j++)
                        {
                            int xi = x + i;
                            int yj = y + j;

                            if (xi < 0 || xi >= width) continue;
                            if (yj < 0 || yj >= height) continue;

                            float h = GetNormalizedHeight(xi, yj);
                            elevations.Add(h);
                        }
                    }

                    float residual = 0;
                    float h0 = GetNormalizedHeight(x, y);
                    float4 color = white;

                    switch (m_residualType)
                    {
                        case RESIDUAL_TYPE.ELEVATION:
                            residual = h0;
                            color = Colorize(residual, 0, true);
                            break;

                        case RESIDUAL_TYPE.MEAN:
                            residual = MeanElevation(elevations);
                            color = Colorize(residual, 0, true);
                            break;

                        case RESIDUAL_TYPE.DIFFERENCE:
                            residual = DifferenceFromMeanElevation(h0, elevations);
                            color = Colorize(residual, 4, false);
                            break;

                        case RESIDUAL_TYPE.STDEV:
                            residual = DeviationFromMeanElevation(h0, elevations);
                            color = Colorize(residual, 0.6f, true);
                            break;

                        case RESIDUAL_TYPE.DEVIATION:
                            residual = DeviationFromMeanElevation(h0, elevations);
                            color = Colorize(residual, 0.6f, false);
                            break;

                        case RESIDUAL_TYPE.PERCENTILE:
                            residual = Percentile(h0, elevations);
                            color = Colorize(residual, 0.3f, true);
                            break;
                    }


                    map[x + y * width] = color;
                }

            }

            return map;
        }

        private float MeanElevation(List<float> elevations)
        {
            return Mean(elevations);
        }

        private float StdevElevation(List<float> elevations)
        {
            var mean = MeanElevation(elevations);
            return math.sqrt(Variance(elevations, mean));
        }

        private float DifferenceFromMeanElevation(float h, List<float> elevations)
        {
            return h - MeanElevation(elevations);
        }

        private float DeviationFromMeanElevation(float h, List<float> elevations)
        {
            var o = StdevElevation(elevations);
            var d = DifferenceFromMeanElevation(h, elevations);

            return (float)FMath.SafeDiv(d, o);
        }

        private float Percentile(float h, List<float> elevations)
        {
            int count = elevations.Count;
            float num = 0;

            for (int i = 0; i < count; i++)
                if (elevations[i] < h) num++;

            if (num == 0) return 0;
            return num / count;
        }

        private float Mean(IList<float> data)
        {
            int count = data.Count;
            if (count == 0) return 0;

            float u = 0;
            for (int i = 0; i < count; i++)
                u += data[i];

            return u / count;
        }

        private float Variance(IList<float> data, float mean)
        {
            int count = data.Count;
            if (count == 0) return 0;

            float v = 0;
            for (int i = 0; i < count; i++)
            {
                float diff = data[i] - mean;
                v += diff * diff;
            }

            return v / count;
        }

    }

}

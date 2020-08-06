﻿using Unity.Mathematics;
using UnityEngine;

namespace TerrainTopology
{
    public enum LANDFORM_TYPE { GAUSSIAN, SHAPE_INDEX, ACCUMULATION };

    [System.Serializable]
    public class CreateLandformMap : CreateTopology
    {

        public LANDFORM_TYPE m_landformType = LANDFORM_TYPE.GAUSSIAN;

        private LANDFORM_TYPE m_currentType;

        protected override bool OnChange()
        {
            return m_currentType != m_landformType || m_currentColorMode != m_coloredGradient;
        }

        /// <summary>
        /// Since landforms uses the second derivatives it can be sensitive to noise.
        /// For best results smooth the heights to reduce noise.
        /// </summary>
        /// <returns></returns>
        protected override bool DoSmoothHeights()
        {
            return true;
        }

        public override Color[] CreateMap()
        {
            m_currentType = m_landformType;

            Color[] map = new Color[m_width * m_height];

            for (int y = 0; y < m_height; y++)
            {
                for (int x = 0; x < m_width; x++)
                {
                    float2 d1;
                    float3 d2;
                    GetDerivatives(x, y, out d1, out d2);

                    float landform = 0;
                    Color color = Color.white;

                    switch (m_landformType)
                    {
                        case LANDFORM_TYPE.GAUSSIAN:
                            landform = GaussianLandform(d1.x, d1.y, d2.x, d2.y, d2.z);
                            color = Colorize(landform, 0, true);
                            break;

                        case LANDFORM_TYPE.SHAPE_INDEX:
                            landform = ShapeIndexLandform(d1.x, d1.y, d2.x, d2.y, d2.z);
                            color = Colorize(landform, 0, true);
                            break;

                        case LANDFORM_TYPE.ACCUMULATION:
                            landform = AccumulationLandform(d1.x, d1.y, d2.x, d2.y, d2.z);
                            color = Colorize(landform, 0, true);
                            break;
                    };

                    map[x + y * m_width] = color;
                }
            }

            return map;
        }

        /// <summary>
        /// Ranges from 0 to 1.
        /// Values > 0.5 relate to convex landforms.
        /// Values < 0.5 relate to concave lanforms.
        /// </summary>
        private float GaussianLandform(float zx, float zy, float zxx, float zyy, float zxy)
        {
            float K = GaussianCurvature(zx, zy, zxx, zyy, zxy);
            float H = MeanCurvature(zx, zy, zxx, zyy, zxy);

            //Hill (dome)
            if (K > 0 && H > 0)
                return 1;

            //Convex saddle
            if (K < 0 && H > 0)
                return 0.75f;

            //Perfect saddle, Antiform (perfect ridge), Synform (perfect valley), Plane.
            //Should be very rare.
            if (K == 0 || H == 0)
                return 0.5f;

            //Concave saddle
            if (K < 0 && H < 0)
                return 0.25f;

            //Depression (Basin)
            if (K > 0 && H < 0)
                return 0;

            throw new System.Exception("Unhandled lanform");
        }

        /// <summary>
        /// Ranges from 0 to 1.
        /// Values > 0.5 relate to convex landforms.
        /// Values < 0.5 relate to concave lanforms.
        /// Same as Gaussian but on a continual sliding scale.
        /// </summary>
        private float ShapeIndexLandform(float zx, float zy, float zxx, float zyy, float zxy)
        {
            float K = GaussianCurvature(zx, zy, zxx, zyy, zxy);
            float H = MeanCurvature(zx, zy, zxx, zyy, zxy);

            float d = FMath.SafeSqrt(H * H - K);

            float si = 2.0f / math.PI * math.atan(FMath.SafeDiv(H, d));

            return si * 0.5f + 0.5f;
        }

        /// <summary>
        /// Ranges from 0 to 1.
        /// value 1 where flows dissperse from.
        /// value 0.75 where flow over convex shape.
        /// value 0.5 where flat.
        /// value 0.25 where flow over concave shape.
        /// value 0 where flows accumalate to.
        /// </summary>
        private float AccumulationLandform(float zx, float zy, float zxx, float zyy, float zxy)
        {
            float Kh = HorizontalCurvature(zx, zy, zxx, zyy, zxy);
            float Kv = VerticalCurvature(zx, zy, zxx, zyy, zxy);

            //Dissipation flows.
            if (Kh > 0 && Kv > 0)
                return 1;

            //Convex transitive.
            if (Kh > 0 && Kv < 0)
                return 0.75f;

            //Planar transitive.
            //Should be very rare.
            if (Kh == 0 || Kv == 0)
                return 0.5f;

            //Concave trasitive.
            if (Kh < 0 && Kv > 0)
                return 0.25f;

            //Accumulative flows.
            if (Kh < 0 && Kv < 0)
                return 0;

            throw new System.Exception("Unhandled lanform");
        }

        /// <summary>
        /// Kh
        /// Same as plan curvature but multiplied by the sine of the slope angle.
        /// Does not take on extremely large values when slope is small.
        /// aka Tangential curvature.
        /// </summary>
        private float HorizontalCurvature(float zx, float zy, float zxx, float zyy, float zxy)
        {
            float zx2 = zx * zx;
            float zy2 = zy * zy;
            float p = zx2 + zy2;

            float n = zy2 * zxx - 2.0f * zxy * zx * zy + zx2 * zyy;
            float d = p * math.pow(p + 1, 0.5f);

            return FMath.SafeDiv(n, d);
        }

        /// <summary>
        /// Kv
        /// Vertical curvature measures the rate of change of the slope.
        /// Is negative for slope increasing downhill and positive for slope decreasing dowhill.
        /// aka profile curvature.
        /// </summary>
        private float VerticalCurvature(float zx, float zy, float zxx, float zyy, float zxy)
        {
            float zx2 = zx * zx;
            float zy2 = zy * zy;
            float p = zx2 + zy2;

            float n = zx2 * zxx + 2.0f * zxy * zx * zy + zy2 * zyy;
            float d = p * math.pow(p + 1, 1.5f);

            return FMath.SafeDiv(n, d);
        }

        /// <summary>
        /// H
        /// Mean curvature represents convergence and relative deceleration with equal weights.
        /// </summary>
        private float MeanCurvature(float zx, float zy, float zxx, float zyy, float zxy)
        {
            float zx2 = zx * zx;
            float zy2 = zy * zy;
            float p = zx2 + zy2;

            float n = (1 + zy2) * zxx - 2.0f * zxy * zx * zy + (1 + zx2) * zyy;
            float d = 2 * math.pow(p + 1, 1.5f);

            return FMath.SafeDiv(n, d);
        }

        /// <summary>
        /// K
        /// Gaussian curvature retains values in each point on the surface after
        /// its bending without breaking, stretching, and compressing.
        /// </summary>
        private float GaussianCurvature(float zx, float zy, float zxx, float zyy, float zxy)
        {
            float zx2 = zx * zx;
            float zy2 = zy * zy;
            float p = zx2 + zy2;

            float n = zxx * zyy - zxy * zxy;
            float d = math.pow(p + 1, 2);

            return FMath.SafeDiv(n, d);
        }


    }

}

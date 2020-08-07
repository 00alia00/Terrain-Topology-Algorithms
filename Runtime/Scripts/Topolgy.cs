using System;
using Unity.Mathematics;

namespace TerrainTopology
{
    public enum VISUALIZE_GRADIENT { WARM, COOL, COOL_WARM, GREY_WHITE, GREY_BLACK, BLACK_WHITE };

    public abstract class Topology
    {
        public static readonly float4 white = new float4(1);
        public static readonly float2 one = new float2(1);

        public bool coloredGradient { get { return m_coloredGradient; } set { m_coloredGradient = value; } }
        protected bool m_coloredGradient;

        private Texture2D m_posGradient, m_negGradient, m_gradient;

        protected bool m_currentColorMode;

        public int width { get { return map.tex_width; } }
        public int height { get { return map.tex_height; } }
        public float[] heights { get { return map.heights; } }

        protected System.Action UpdateMap;

        public void SetUpdateMap(Action updateMapTexture)
        {
            UpdateMap = updateMapTexture;
        }

        public struct MapData
        {
            public float terrain_width;
            public float terrain_height;
            public float terrain_length;

            public int tex_width;
            public int tex_height;

            public float cell_length;

            public float[] heights;
        }

        protected MapData map;

        public void Start(MapData map, System.Action UpdateMap)
        {
            this.map = map;
            this.UpdateMap = UpdateMap;

            //Create color gradients to help visualize the maps.
            m_currentColorMode = m_coloredGradient;

            CreateGradients(m_coloredGradient);

            //If required smooth the heights.
            if (DoSmoothHeights())
                SmoothHeightMap();

            UpdateMap();
        }

        public void Update()
        {
            //If settings changed then recreate map.
            if (OnChange())
            {
                CreateGradients(m_coloredGradient);
                UpdateMap();

                m_currentColorMode = m_coloredGradient;
            }
        }

        /// <summary>
        /// Default mode is nothing changes.
        /// </summary>
        /// <returns></returns>
        protected virtual bool OnChange()
        {
            return false;
        }

        /// <summary>
        /// Default mode is no smoothing.
        /// </summary>
        /// <returns></returns>
        protected virtual bool DoSmoothHeights()
        {
            return false;
        }

        /// <summary>
        /// Create the map. Update to derivered class to implement.
        /// </summary>
        public abstract float4[] CreateMap();

        /// <summary>
        /// Load the provided height map.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bigendian"></param>
        /// <returns></returns>
        public static float[] Load16Bit(string fileName, bool bigendian = false)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(fileName);

            int size = bytes.Length / 2;
            float[] data = new float[size];

            for (int x = 0, i = 0; x < size; x++)
            {
                data[x] = (bigendian) ? (bytes[i++] * 256.0f + bytes[i++]) : (bytes[i++] + bytes[i++] * 256.0f);
                data[x] /= ushort.MaxValue;
            }

            return data;
        }

        /// <summary>
        /// Get a hight value ranging from 0 - 1.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected float GetNormalizedHeight(int x, int y)
        {
            x = math.clamp(x, 0, map.tex_width - 1);
            y = math.clamp(y, 0, map.tex_height - 1);

            return map.heights[x + y * map.tex_width];
        }

        /// <summary>
        /// Get a hight value ranging from 0 - actaul height in meters.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected float GetHeight(int x, int y)
        {
            return GetNormalizedHeight(x, y) * map.terrain_height;
        }

        /// <summary>
        /// Get the heigts maps first derivative using Evans-Young method.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected float2 GetFirstDerivative(int x, int y)
        {
            float w = map.cell_length;
            float z1 = GetHeight(x - 1, y + 1);
            float z2 = GetHeight(x + 0, y + 1);
            float z3 = GetHeight(x + 1, y + 1);
            float z4 = GetHeight(x - 1, y + 0);
            float z6 = GetHeight(x + 1, y + 0);
            float z7 = GetHeight(x - 1, y - 1);
            float z8 = GetHeight(x + 0, y - 1);
            float z9 = GetHeight(x + 1, y - 1);

            //p, q
            float zx = (z3 + z6 + z9 - z1 - z4 - z7) / (6.0f * w);
            float zy = (z1 + z2 + z3 - z7 - z8 - z9) / (6.0f * w);

            return new float2(-zx, -zy);
        }

        /// <summary>
        /// Get the heigts maps first and second derivative using Evans-Young method.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        protected void GetDerivatives(int x, int y, out float2 d1, out float3 d2)
        {
            float w = map.cell_length;
            float w2 = w * w;
            float z1 = GetHeight(x - 1, y + 1);
            float z2 = GetHeight(x + 0, y + 1);
            float z3 = GetHeight(x + 1, y + 1);
            float z4 = GetHeight(x - 1, y + 0);
            float z5 = GetHeight(x + 0, y + 0);
            float z6 = GetHeight(x + 1, y + 0);
            float z7 = GetHeight(x - 1, y - 1);
            float z8 = GetHeight(x + 0, y - 1);
            float z9 = GetHeight(x + 1, y - 1);

            //p, q
            float zx = (z3 + z6 + z9 - z1 - z4 - z7) / (6.0f * w);
            float zy = (z1 + z2 + z3 - z7 - z8 - z9) / (6.0f * w);

            //r, t, s
            float zxx = (z1 + z3 + z4 + z6 + z7 + z9 - 2.0f * (z2 + z5 + z8)) / (3.0f * w2);
            float zyy = (z1 + z2 + z3 + z7 + z8 + z9 - 2.0f * (z4 + z5 + z6)) / (3.0f * w2);
            float zxy = (z3 + z7 - z1 - z9) / (4.0f * w2);

            d1 = new float2(-zx, -zy);
            d2 = new float3(-zxx, -zyy, -zxy); //is zxy or -zxy?
        }

        /// <summary>
        /// Smooth heights using a 5X5 Gaussian kernel.
        /// </summary>
        protected void SmoothHeightMap()
        {
            var heights = new float[map.tex_width * map.tex_height];

            var gaussianKernel5 = new float[,]
            {
                {1,4,6,4,1},
                {4,16,24,16,4},
                {6,24,36,24,6},
                {4,16,24,16,4},
                {1,4,6,4,1}
            };

            float gaussScale = 1.0f / 256.0f;

            for (int y = 0; y < map.tex_height; y++)
            {
                for (int x = 0; x < map.tex_width; x++)
                {
                    float sum = 0;

                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            int xi = x - 2 + i;
                            int yi = y - 2 + j;

                            sum += GetNormalizedHeight(xi, yi) * gaussianKernel5[i, j] * gaussScale;
                        }
                    }

                    heights[x + y * map.tex_width] = sum;
                }
            }

            map.heights = heights;
        }

        /// <summary>
        /// Take a parameter, rescale it and return as a 
        /// color using a gradient. Helps visualize some 
        /// parameters better especially if they have a 
        /// wide dynamic range and can be negative. 
        /// </summary>
        /// <param name="v">The parameter</param>
        /// <param name="exponent">Amount to rescale the dynamic range. 
        /// Will change if terrain cell length changes.</param>
        /// <param name="nonNegative">If the parameter is always positive</param>
        /// <returns></returns>
        protected float4 Colorize(float v, float exponent, bool nonNegative)
        {
            if (exponent > 0)
            {
                float sign = FMath.SignOrZero(v);
                float pow = math.pow(10, exponent);
                float log = math.log(1.0f + pow * math.abs(v));

                v = sign * log;
            }

            if (nonNegative)
                return m_gradient.GetPixelBilinear(v, 0);
            else
            {
                if (v > 0)
                    return m_posGradient.GetPixelBilinear(v, 0);
                else
                    return m_negGradient.GetPixelBilinear(-v, 0);
            }
        }

        public class Texture2D
        {
            float4[] texture;

            int2 size;

            public Texture2D(int width, int height)
            {
                size = new int2(width, height);
                texture = new float4[width * height];
            }

            public void SetPixel(int x, int y, float4 value, bool need_scale = true)
            {
                if (need_scale)
                {
                    value *= 1.0f / 255.0f;
                }

                texture[y * size.x + x] = value;
            }

            public float4 GetPixelBilinear(float u, float v)
            {
                // Pixel centers
                var pcu = u * size.x - 0.5f;
                var pcv = v * size.y - 0.5f;

                float2 pixel = new float2(pcu, pcv);

                float2 bound = size - one;

                // Offset to get 4 closest to pixel
                float2 p0 = math.clamp(math.floor(pixel), float2.zero, bound);
                float2 p1 = math.clamp(p0 + new float2(0, 1), float2.zero, bound);
                float2 p2 = math.clamp(p0 + new float2(1, 0), float2.zero, bound);
                float2 p3 = math.clamp(p0 + new float2(1, 1), float2.zero, bound);

                // get the values at each pixel
                float4 v0 = texture[(int)(p0.y * size.x) + (int)p0.x];
                float4 v1 = texture[(int)(p1.y * size.x) + (int)p1.x];
                float4 v2 = texture[(int)(p2.y * size.x) + (int)p2.x];
                float4 v3 = texture[(int)(p3.y * size.x) + (int)p3.x];

                // Calculate x
                var R1 = math.lerp(v0, v2, (pcu-p0.x));
                var R2 = math.lerp(v1, v3, (pcu-p0.x));
                
                // Calculate y
                var P = math.lerp(R1, R2, (pcv-p0.y ));

                // Clamp between 0 and 1
                return math.clamp(P, 0, 1);
            }
        }

        private void CreateGradients(bool colored)
        {
            if (colored)
            {
                m_gradient = CreateGradient(VISUALIZE_GRADIENT.COOL_WARM);
                m_posGradient = CreateGradient(VISUALIZE_GRADIENT.WARM);
                m_negGradient = CreateGradient(VISUALIZE_GRADIENT.COOL);
            }
            else
            {
                m_gradient = CreateGradient(VISUALIZE_GRADIENT.BLACK_WHITE);
                m_posGradient = CreateGradient(VISUALIZE_GRADIENT.GREY_WHITE);
                m_negGradient = CreateGradient(VISUALIZE_GRADIENT.GREY_BLACK);
            }
        }

        private Texture2D CreateGradient(VISUALIZE_GRADIENT g)
        {
            switch (g)
            {
                case VISUALIZE_GRADIENT.WARM:
                    return CreateWarmGradient();

                case VISUALIZE_GRADIENT.COOL:
                    return CreateCoolGradient();

                case VISUALIZE_GRADIENT.COOL_WARM:
                    return CreateCoolToWarmGradient();

                case VISUALIZE_GRADIENT.GREY_WHITE:
                    return CreateGreyToWhiteGradient();

                case VISUALIZE_GRADIENT.GREY_BLACK:
                    return CreateGreyToBlackGradient();

                case VISUALIZE_GRADIENT.BLACK_WHITE:
                    return CreateBlackToWhiteGradient();
            }

            return null;
        }

        private Texture2D CreateWarmGradient()
        {
            var gradient = new Texture2D(5, 1);
            gradient.SetPixel(0, 0, new float4(80, 230, 80, 255));
            gradient.SetPixel(1, 0, new float4(180, 230, 80, 255));
            gradient.SetPixel(2, 0, new float4(230, 230, 80, 255));
            gradient.SetPixel(3, 0, new float4(230, 180, 80, 255));
            gradient.SetPixel(4, 0, new float4(230, 80, 80, 255));

            return gradient;
        }

        private Texture2D CreateCoolGradient()
        {
            var gradient = new Texture2D(5, 1);
            gradient.SetPixel(0, 0, new float4(80, 230, 80, 255));
            gradient.SetPixel(1, 0, new float4(80, 230, 180, 255));
            gradient.SetPixel(2, 0, new float4(80, 230, 230, 255));
            gradient.SetPixel(3, 0, new float4(80, 180, 230, 255));
            gradient.SetPixel(4, 0, new float4(80, 80, 230, 255));

            return gradient;
        }

        private Texture2D CreateCoolToWarmGradient()
        {
            var gradient = new Texture2D(9, 1);
            gradient.SetPixel(0, 0, new float4(80, 80, 230, 255));
            gradient.SetPixel(1, 0, new float4(80, 180, 230, 255));
            gradient.SetPixel(2, 0, new float4(80, 230, 230, 255));
            gradient.SetPixel(3, 0, new float4(80, 230, 180, 255));
            gradient.SetPixel(4, 0, new float4(80, 230, 80, 255));
            gradient.SetPixel(5, 0, new float4(180, 230, 80, 255));
            gradient.SetPixel(6, 0, new float4(230, 230, 80, 255));
            gradient.SetPixel(7, 0, new float4(230, 180, 80, 255));
            gradient.SetPixel(8, 0, new float4(230, 80, 80, 255));

            return gradient;
        }

        private Texture2D CreateGreyToWhiteGradient()
        {
            var gradient = new Texture2D(3, 1);
            gradient.SetPixel(0, 0, new float4(128, 128, 128, 255));
            gradient.SetPixel(1, 0, new float4(192, 192, 192, 255));
            gradient.SetPixel(2, 0, new float4(255, 255, 255, 255));

            return gradient;
        }

        private Texture2D CreateGreyToBlackGradient()
        {
            var gradient = new Texture2D(3, 1);
            gradient.SetPixel(0, 0, new float4(128, 128, 128, 255));
            gradient.SetPixel(1, 0, new float4(64, 64, 64, 255));
            gradient.SetPixel(2, 0, new float4(0, 0, 0, 255));

            return gradient;
        }

        private Texture2D CreateBlackToWhiteGradient()
        {
            var gradient = new Texture2D(5, 1);
            gradient.SetPixel(0, 0, new float4(0, 0, 0, 255));
            gradient.SetPixel(1, 0, new float4(64, 64, 64, 255));
            gradient.SetPixel(2, 0, new float4(128, 128, 128, 255));
            gradient.SetPixel(3, 0, new float4(192, 192, 192, 255));
            gradient.SetPixel(4, 0, new float4(255, 255, 255, 255));

            return gradient;
        }

    }

}

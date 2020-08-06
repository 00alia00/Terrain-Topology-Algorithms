using Unity.Mathematics;

namespace TerrainTopology
{
    public class CreateNormalMap : CreateTopology
    {
        public override float4[] CreateMap()
        {
            float4[] map = new float4[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float2 d1 = GetFirstDerivative(x, y);

                    //Not to sure of the orientation.
                    //Might need to flip x or y

                    var n = new float3();
                    n.x = d1.x * 0.5f + 0.5f;
                    n.y = -d1.y * 0.5f + 0.5f; 
                    n.z = 1.0f;

                    n = math.normalize(n);

                    map[x + y * width] = new float4(n.x, n.y, n.z, 1);
                }
            }

            return map;
        }

    }
}

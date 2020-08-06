using Unity.Mathematics;

using UnityEngine;

namespace TerrainTopology
{

    [System.Serializable]
    public class CreateNormalMap : CreateTopology
    {
        public override Color[] CreateMap()
        {
            Color[] map = new Color[m_width * m_height];

            for (int y = 0; y < m_height; y++)
            {
                for (int x = 0; x < m_width; x++)
                {
                    float2 d1 = GetFirstDerivative(x, y);

                    //Not to sure of the orientation.
                    //Might need to flip x or y

                    var n = new float3();
                    n.x = d1.x * 0.5f + 0.5f;
                    n.y = -d1.y * 0.5f + 0.5f; 
                    n.z = 1.0f;

                    n = math.normalize(n);

                    map[x + y * m_width] = new Color(n.x, n.y, n.z, 1);
                }
            }

            return map;
        }

    }
}

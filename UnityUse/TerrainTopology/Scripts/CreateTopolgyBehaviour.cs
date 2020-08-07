using UnityEngine;

namespace TerrainTopology
{
    public abstract class CreateTopologyBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected CreateTopology topology;

        [SerializeField]
        protected bool coloredGradient;

        public Material m_material;

        public void Start()
        {
            if (m_material == null) return;

            topology.coloredGradient = coloredGradient;
            topology.Start(new CreateTopology.MapData()
            {            
                //The loaded heights map is a 16 bit 1024 by 1024 raw image
                m_width = 1024,
                m_height = 1024,

                //The terrain is about a 10Km square and about 2Km from lowest to highest point.
                m_terrainWidth = 10000,
                m_terrainHeight = 2000,
                m_terrainLength = 10000,

                //That makes each pixel in height map about 10m in length.
                m_cellLength = 10,

                m_heights = CreateTopology.Load16Bit(Application.dataPath + "/TerrainTopology/Heights.raw")
            }, UpdateMap);
        }

        public void OnDestroy()
        {
            if (m_material == null) return;
            m_material.mainTexture = null;
        }

        public void Update()
        {
            topology.Update();
        }

        protected void UpdateMap()
        {
            Texture2D slopeMap = new Texture2D(topology.width, topology.height);

            var map = topology.CreateMap();
            for (int y = 0; y < topology.height; y++)
            {
                for (int x = 0; x < topology.width; x++)
                {
                    var val = map[x + y * topology.width];
                    slopeMap.SetPixel(x, y, new Color(val.x, val.y, val.z, val.w));
                }
            }

            slopeMap.Apply();
            m_material.mainTexture = slopeMap;
        }
    }
}

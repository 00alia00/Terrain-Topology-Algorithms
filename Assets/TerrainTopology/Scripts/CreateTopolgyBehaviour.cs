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
            topology.Start(Application.dataPath + "/TerrainTopology/Heights.raw", UpdateMap);
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
                    slopeMap.SetPixel(x, y, map[x + y * topology.width]);
                }
            }

            slopeMap.Apply();
            m_material.mainTexture = slopeMap;
        }
    }
}

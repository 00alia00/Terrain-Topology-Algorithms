namespace TerrainTopology
{
    public class CreateNormalMapBehaviour : CreateTopologyBehaviour
    {
        public new void Start()
        {
            topology = new NormalMap();
            base.Start();
        }
    }
}

namespace TerrainTopology
{
    public class CreateNormalMapBehaviour : CreateTopologyBehaviour
    {
        public new void Start()
        {
            topology = new CreateNormalMap();
            base.Start();
        }
    }
}

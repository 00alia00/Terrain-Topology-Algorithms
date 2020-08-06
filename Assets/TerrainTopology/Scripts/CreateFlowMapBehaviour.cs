namespace TerrainTopology
{
    public class CreateFlowMapBehaviour : CreateTopologyBehaviour
    {
        public new void Start()
        {
            topology = new CreateFlowMap();
            base.Start();
        }
    }
}

namespace TerrainTopology
{
    public class CreateSlopeMapBehaviour : CreateTopologyBehaviour
    {
        public new void Start()
        {
            topology = new CreateSlopeMap();
            base.Start();
        }
    }
}
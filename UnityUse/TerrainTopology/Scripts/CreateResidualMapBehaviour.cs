namespace TerrainTopology
{
    public class CreateResidualMapBehaviour : CreateTopologyBehaviour
    {
        public new void Start()
        {
            topology = new CreateResidualMap();
            base.Start();
        }
    }
}

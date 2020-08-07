namespace TerrainTopology
{
    public class CreateResidualMapBehaviour : CreateTopologyBehaviour
    {
        public new void Start()
        {
            topology = new ResidualMap();
            base.Start();
        }
    }
}

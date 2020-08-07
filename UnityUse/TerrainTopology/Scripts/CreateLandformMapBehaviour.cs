namespace TerrainTopology
{
    public class CreateLandformMapBehaviour : CreateTopologyBehaviour
    {
        public new void Start()
        {
            topology = new CreateLandformMap();
            base.Start();
        }
    }
}

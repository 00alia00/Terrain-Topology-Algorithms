namespace TerrainTopology
{
    public class CreateLandformMapBehaviour : CreateTopologyBehaviour
    {
        public new void Start()
        {
            topology = new LandformMap();
            base.Start();
        }
    }
}

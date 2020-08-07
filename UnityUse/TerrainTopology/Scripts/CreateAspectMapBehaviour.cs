namespace TerrainTopology
{
    public class CreateAspectMapBehaviour : CreateTopologyBehaviour
    {
        public new void Start()
        {
            topology = new AspectMap();
            base.Start();
        }
    }
}

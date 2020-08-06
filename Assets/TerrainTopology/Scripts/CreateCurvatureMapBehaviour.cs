namespace TerrainTopology
{
    public class CreateCurvatureMapBehaviour : CreateTopologyBehaviour
    {
        public new void Start()
        {
            topology = new CreateCurvatureMap();
            base.Start();
        }
    }
}
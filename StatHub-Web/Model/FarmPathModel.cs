using SqlSugar;

namespace StatHub.Web.Model
{
    [SugarTable("FarmPathTable")]
    public class FarmPathModel
    {
        public FarmPathModel()
        {
        }
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        public int FarmId { get; set; }
        public string Size { get; set; }
        public string Path { get; set; }
    }
}

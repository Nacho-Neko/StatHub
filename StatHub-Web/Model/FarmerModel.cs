using SqlSugar;

namespace StatHub.Web.Model
{
    [SugarTable("FarmerTable")]
    public class FarmerModel
    {
        private readonly ISqlSugarClient sqlSugarClient;
        public FarmerModel()
        {
        }
        public FarmerModel(ISqlSugarClient sqlSugarClient)
        {
            this.sqlSugarClient = sqlSugarClient;
        }

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }

        public DateTime Update_at { get; set; }

        [Navigate(NavigateType.OneToMany, nameof(FarmPathModel.FarmId))]
        public List<FarmPathModel> farmPaths { get; set; }

        public async Task<List<FarmerModel>> Farmers() {
            return await sqlSugarClient.Queryable<FarmerModel>().Includes(it => it.farmPaths).ToListAsync();
        }
        public async Task<FarmerModel> GetFarm(string Name)
        {
            try
            {
                FarmerModel farmerModel = await sqlSugarClient.Queryable<FarmerModel>().Where(it => it.Name == Name).Includes(it => it.farmPaths).FirstAsync();

                farmerModel.Id = farmerModel.Id;
                farmerModel.Guid = farmerModel.Guid;
                farmerModel.Name = farmerModel.Name;

                return farmerModel;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

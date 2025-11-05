namespace WebAPIImport.DAO.Interface
{
    public interface IRecordDataDataBase
    {
        public Task<Models.RecordData?> GetAsync(Guid id);

        public Task<List<Models.RecordData>> ListAsync();

        public Task<Models.RecordData> SaveAsync(Models.RecordData item);

        public Task<Models.RecordData> DeleteAsync(Models.RecordData item);
    }
}

using WebAPIImport.Models;

namespace WebAPIImport.Logic.Interface
{
    public interface IRecordDataLogic
    {
        public Task<RecordData> CreateAsync(RecordData model);
        public Task<RecordData> UpdateAsync(RecordData model);
        public Task<RecordData?> DeleteAsync(Guid id);
        public Task<RecordData?> GetAsync(Guid id);
        public Task<List<RecordData>> ListAsync();
    }
}

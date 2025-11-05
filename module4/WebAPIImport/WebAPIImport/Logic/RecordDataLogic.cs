using WebAPIImport.Adapters.Interface;
using WebAPIImport.DAO;
using WebAPIImport.DAO.Interface;
using WebAPIImport.Logic.Interface;
using WebAPIImport.Models;

namespace WebAPIImport.Logic
{
    public class RecordDataLogic : IRecordDataLogic
    {
        private ILogger<RecordDataLogic> Logger;
        private IRecordDataDataBase RecordDataDataBase;
        private IRecordDataAdapter RecordDataAdapter;

        public RecordDataLogic(ILogger<RecordDataLogic> logger, IRecordDataDataBase recordDataDataBase, IRecordDataAdapter recordDataAdapter)
        {
            Logger = logger;
            RecordDataDataBase = recordDataDataBase;
            RecordDataAdapter = recordDataAdapter;
        }

        public async Task<RecordData?> DeleteAsync(Guid id)
        {
            Logger.LogInformation($"Deleting RecordData with id {id}");
            RecordData? model = GetAsync(id).Result;
            if (model != null)
            {
                return RecordDataAdapter.DAOToModel(await RecordDataDataBase.DeleteAsync(RecordDataAdapter.ToDAOModel(model)));
            }
            return null;
        }

        public async Task<RecordData?> GetAsync(Guid id)
        {
            Logger.LogInformation($"Getting RecordData with id {id}");
            var data = await RecordDataDataBase.GetAsync(id);
            if (data != null)
            {
                return RecordDataAdapter.DAOToModel(data);
            }
            return null;
        }

        public async Task<List<RecordData>> ListAsync()
        {
            Logger.LogInformation($"Listing RecordData");
            return RecordDataAdapter.DAOToModelList(await RecordDataDataBase.ListAsync());
        }

        public async Task<RecordData> CreateAsync(RecordData model)
        {
            Logger.LogInformation($"Creating RecordData");
            return RecordDataAdapter.DAOToModel(await RecordDataDataBase.SaveAsync(RecordDataAdapter.ToDAOModel(model)));
        }

        public async Task<RecordData> UpdateAsync(RecordData model)
        {
            Logger.LogInformation($"Updating RecordData with id {model.id}");
            return RecordDataAdapter.DAOToModel(await RecordDataDataBase.SaveAsync(RecordDataAdapter.ToDAOModel(await RecordDataDataBase.GetAsync(model.id), model)));
        }
    }
}


namespace WebAPIImport.Adapters.Interface
{
    public interface IRecordDataAdapter
    {
        public Models.RecordData DAOToModel(DAO.Models.RecordData daoModel);

        public DAO.Models.RecordData ToDAOModel(Models.RecordData model);

        public DAO.Models.RecordData ToDAOModel(DAO.Models.RecordData daoModel, Models.RecordData model);

        public List<Models.RecordData> DAOToModelList(List<DAO.Models.RecordData> daoModelList);

        public List<DAO.Models.RecordData> ToDAOModelList(List<Models.RecordData> daoModelList);

        public API.Models.RecordDataOut ToAPIModel(Models.RecordData model);

        public Models.RecordData APIToModel(API.Models.RecordDataIn apiModel);

        public Models.RecordData APIToModel(API.Models.RecordDataIn apiModel, Guid id);

        public List<API.Models.RecordDataOut> ToAPIModelList(List<Models.RecordData> modelList);
    }
}

using Microsoft.EntityFrameworkCore.Metadata;
using WebAPIImport.Adapters.Interface;
using WebAPIImport.API.Models;
using WebAPIImport.Models;

namespace WebAPIImport.Adapters
{
    public class RecordDataAdapter : IRecordDataAdapter
    {
        public RecordData APIToModel(RecordDataIn apiModel)
        {
            return APIToModel(apiModel, new Guid());
        }

        public RecordData APIToModel(RecordDataIn apiModel, Guid id)
        {
            Models.RecordData returnModel = new Models.RecordData()
            {
                id = id,
                Value1 = apiModel.value1,
                Value2 = apiModel.value2,
                Value3 = apiModel.value3
            };

            return returnModel;
        }

        public Models.RecordData DAOToModel(DAO.Models.RecordData daoModel)
        {
            Models.RecordData returnModel = new Models.RecordData()
            {
                id = daoModel.id,
                Value1 = daoModel.Value1,
                Value2 = daoModel.Value2,
                Value3 = daoModel.Value3
            };

            return returnModel;
        }

        public List<Models.RecordData> DAOToModelList(List<DAO.Models.RecordData> daoModelList)
        {
            return daoModelList.Select(daoModel => DAOToModel(daoModel)).ToList();
        }

        public RecordDataOut ToAPIModel(RecordData model)
        {
            RecordDataOut returnModel = new RecordDataOut()
            {
                id = model.id,
                value1 = model.Value1,
                value2 = model.Value2,
                value3 = model.Value3
            };
            return returnModel;
        }

        public List<RecordDataOut> ToAPIModelList(List<RecordData> modelList)
        {
            return modelList.Select(model => ToAPIModel(model)).ToList();
        }

        public DAO.Models.RecordData ToDAOModel(Models.RecordData model)
        {
            DAO.Models.RecordData returnModel = new DAO.Models.RecordData()
            {
                id = model.id,
                Value1 = model.Value1,
                Value2 = model.Value2,
                Value3 = model.Value3
            };
            return returnModel;
        }

        public DAO.Models.RecordData ToDAOModel(DAO.Models.RecordData daoModel, RecordData model)
        {
            daoModel.Value1 = model.Value1;
            daoModel.Value2 = model.Value2;
            daoModel.Value3 = model.Value3;

            return daoModel;
        }

        public List<DAO.Models.RecordData> ToDAOModelList(List<Models.RecordData> modelList)
        {
            return modelList.Select(model => ToDAOModel(model)).ToList();
        }
    }
}

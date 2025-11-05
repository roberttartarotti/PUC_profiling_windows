using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPIImport.Adapters.Interface;
using WebAPIImport.API.Models;
using WebAPIImport.DAO.Models;
using WebAPIImport.Logic.Interface;

namespace WebAPIImport.API.Controllers
{
    [ApiController]
    [Route("api/recorddata")]
    public class RecordDataController : ControllerBase
    {
        private readonly ILogger<RecordDataController> Logger;
        private readonly IRecordDataAdapter RecordDataAdapter;
        private readonly IRecordDataLogic RecordDataLogic;

        public RecordDataController(ILogger<RecordDataController> logger, IRecordDataAdapter recordDataAdapter, IRecordDataLogic recordDataLogic)
        {
            Logger = logger;
            RecordDataAdapter = recordDataAdapter;
            RecordDataLogic = recordDataLogic;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get()
        {
            WebAPIImportApp.Log.ExecuteMethod("GET");
            return StatusCode(200, RecordDataAdapter.ToAPIModelList(await RecordDataLogic.ListAsync()));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetId(Guid id)
        {
            WebAPIImportApp.Log.ExecuteMethod("GET");
            Logger.LogInformation(1200, $"Getting RecordData with id {id}");
            var model = await RecordDataLogic.GetAsync(id);
            if (model == null)
            {
                return StatusCode(404, "Not Found");
            }
            return StatusCode(200, RecordDataAdapter.ToAPIModel(model));
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Post([FromBody] RecordDataIn data)
        {
            try
            {
                WebAPIImportApp.Log.ExecuteMethod("POST");
                Logger.LogInformation(1200, "Creating new RecordData");
                var model = RecordDataAdapter.APIToModel(data);
                model = await RecordDataLogic.CreateAsync(model);
                return StatusCode(202, RecordDataAdapter.ToAPIModel(model));
            }
            catch (Exception ex)
            {
                WebAPIImportApp.Log.ErrorMethod("POST");
                Logger.LogError(500, ex, "Error creating RecordData");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] RecordDataIn data)
        {
            WebAPIImportApp.Log.ExecuteMethod("PUT");
            Logger.LogInformation(1200, $"Updating RecordData with id {id}");
            var model = await RecordDataLogic.GetAsync(id);
            if (model == null)
            {
                return StatusCode(404, "Not Found");
            }
            model = RecordDataAdapter.APIToModel(data, id);
            model = await RecordDataLogic.UpdateAsync(model);
            return StatusCode(200, RecordDataAdapter.ToAPIModel(model));
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            WebAPIImportApp.Log.ExecuteMethod("DELETE");
            Logger.LogInformation(1200, $"Deleting RecordData with id {id}");
            var model = await RecordDataLogic.DeleteAsync(id);
            if (model == null)
            {
                return StatusCode(404, "Not Found");
            }
            return StatusCode(200, RecordDataAdapter.ToAPIModel(model));
        }
    }
}

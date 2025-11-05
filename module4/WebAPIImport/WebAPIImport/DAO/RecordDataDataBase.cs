using Microsoft.EntityFrameworkCore;
using WebAPIImport.DAO.Interface;
using WebAPIImport.DAO.Models;

namespace WebAPIImport.DAO
{
    public class RecordDataDataBase : IRecordDataDataBase
    {
        private ILogger<RecordDataDataBase> Logger;
        private DataContext Context;

        public RecordDataDataBase(ILogger<RecordDataDataBase> logger, DataContext context)
        {
            Logger = logger;
            Context = context;
        }

        public async Task<RecordData> DeleteAsync(RecordData item)
        {
            Logger.LogInformation($"Deleting RecordData with id {item.id}");
            try
            {
                Context.RecordsData.Remove(item);
                await Context.SaveChangesAsync();
                return item;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,$"Error deleting RecordData with id {item.id}");
                throw new Exception($"Error deleting RecordData with id {item.id}", ex);
            }

        }

        public async Task<RecordData?> GetAsync(Guid id)
        {
            Logger.LogInformation($"Getting RecordData with id {id}");
            try
            {
                return await QueryBase().FirstOrDefaultAsync(x => x.id == id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error geting RecordData with id {id}");
                throw new Exception($"Error geting RecordData with id {id}", ex);
            }
        }

        public async Task<List<RecordData>> ListAsync()
        {
            Logger.LogInformation($"Listing RecordData");
            try
            {
                return await QueryBase().ToListAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error listing RecordData");
                throw new Exception($"Error listing RecordData", ex);
            }
        }

        public async Task<RecordData> SaveAsync(RecordData item)
        {
            Logger.LogInformation($"Saving RecordData with id {item.id}");
            try
            {
                item.CreateAt = DateTime.UtcNow;

                if (item.id == new Guid())
                {
                    item.id = Guid.NewGuid();
                    await Context.RecordsData.AddAsync(item);
                }
                else
                {
                    Context.RecordsData.Update(item);
                }

                await Context.SaveChangesAsync();
                return item;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error saving RecordData with id {item.id}");
                throw new Exception($"Error saving RecordData with id {item.id}", ex);
            }
        }

        private IQueryable<RecordData> QueryBase()
        {
            return Context.RecordsData.AsQueryable();
        }
    }
}

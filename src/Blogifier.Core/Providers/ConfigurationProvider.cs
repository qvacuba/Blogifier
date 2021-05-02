using System.Collections.ObjectModel;
using Blogifier.Core.Data;
using Blogifier.Shared;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blogifier.Core.Providers
{

    public interface IConfigurationProvider {
        Task<Configuration> GetConfiguration(string key);
        Task<List<Configuration>> GetAllConfigurations();

        Task<Configuration> Add(string key, bool value);
        Task<bool> Update(string key, bool value);
    }
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly AppDbContext _db;
		public ConfigurationProvider(AppDbContext db)
		{
			_db = db;
		}
        public async Task<Configuration> Add(string key, bool value)
        {
            var entity = await _db.Configurations.Where(c => c.Name == key).FirstOrDefaultAsync();
            if (entity != null) {
                entity = new Configuration(){ Name = key, Active = value};
                await _db.Configurations.AddAsync(entity);
                await _db.SaveChangesAsync();
            }
            return entity;
        }

        public async Task<List<Configuration>> GetAllConfigurations()
        {
            return await _db.Configurations.ToListAsync();
        }

        public async Task<Configuration> GetConfiguration(string key)
        {
            return await _db.Configurations.Where(c => c.Name == key).FirstOrDefaultAsync();
        }

        public async Task<bool> Update(string key, bool value)
        {
            var entity = await _db.Configurations.Where(c => c.Name == key).FirstOrDefaultAsync();
            if (entity != null) {
                entity.Active = value;
                _db.Configurations.Update(entity);
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}

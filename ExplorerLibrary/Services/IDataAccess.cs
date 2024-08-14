using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerLibrary.Services
{
    public interface IDataAccess
    {
        Task SaveData<T>(string connectionString, T data, string sql);
        Task<T> ReadData<T>(string sql, string connectionString);
    }
}

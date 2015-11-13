using SqlWebConsole.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Dapper;

namespace SqlWebConsole.ApiControllers
{
    public class SqlController : ApiController
    {
        public async Task<IHttpActionResult> Post(CommandModel command)
        {

            try
            {
                IEnumerable<object> result = null;
                var provider = System.Data.Common.DbProviderFactories.GetFactory(command.Connection.ProviderName);
                var csb = provider.CreateConnectionStringBuilder();
                csb.ConnectionString = command.Connection.ConnectionString;

                using (var connection = provider.CreateConnection())
                {
                    if (!string.IsNullOrWhiteSpace(command.Connection.Username))
                    {
                        csb["User Id"] = command.Connection.Username;
                    }

                    if (!string.IsNullOrWhiteSpace(command.Connection.Password))
                    {
                        csb["Password"] = command.Connection.Password;
                    }

                    connection.ConnectionString = csb.ConnectionString;

                    await connection.OpenAsync();

                    result = (await connection.QueryAsync(command.CommandText)).Take(500).ToArray();
                    
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}

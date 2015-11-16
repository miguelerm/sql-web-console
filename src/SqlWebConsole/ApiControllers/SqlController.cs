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
                var result = new CommandResultModel();
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

                    var dbCommand = connection.CreateCommand();
                    dbCommand.CommandText = command.CommandText;


                    using (var reader = await dbCommand.ExecuteReaderAsync())
                    {

                        result.RecordsAffected = reader.RecordsAffected;

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var fieldName = reader.GetName(i);
                            var fieldType = reader.GetFieldType(i).FullName;
                            var fieldDataType = reader.GetDataTypeName(i);
                            result.Columns.Add(fieldName, new ColumnModel { Name = fieldName, Type = fieldType, DataType = fieldDataType });
                        }

                        while (await reader.ReadAsync())
                        {
                            var item = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var fieldName = reader.GetName(i);
                                var fieldValue = reader.GetValue(i);
                                item.Add(fieldName, fieldValue);
                            }
                            result.Items.Add(item);
                        }
                    }

                    //result = (await connection.QueryAsync(command.CommandText)).Take(500).ToArray();
                    
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

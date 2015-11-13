using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SqlWebConsole.ApiControllers
{
    public class ProvidersController : ApiController
    {
        public IHttpActionResult Get()
        {
            var factoriesTable = DbProviderFactories.GetFactoryClasses();

            var providers = new List<Dictionary<string, string>>(factoriesTable.Rows.Count);

            foreach (DataRow row in factoriesTable.Rows)
            {
                var factory = new Dictionary<string, string>(factoriesTable.Columns.Count);
                foreach (DataColumn column in factoriesTable.Columns)
                {
                    factory.Add(column.ColumnName, row[column] as string);
                }

                providers.Add(factory);
            }

            return Ok(providers);
        }
    }
}

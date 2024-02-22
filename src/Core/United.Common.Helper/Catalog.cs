using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.Model.Common;
using United.Utility.Helper;
using Microsoft.Extensions.Logging;

namespace United.Common.Helper
{
    public class Catalog : ICatalog
    {
        private readonly ICacheLog<Catalog> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDynamoDBHelperService _dynamoDB;

        public Catalog(
              ICacheLog<Catalog> logger
            , IConfiguration configuration
            , IDynamoDBHelperService dynamoDB)
        {
            _logger = logger;
            _configuration = configuration;
            _dynamoDB = dynamoDB;
        }

        public async Task<List<MOBItem>> GetCatalogItems(int applicationId, string deviceId = "")
        {
            List<MOBItem> items = null;
            string storedproc = "uasp_Select_CatalogItems"; // this is th default proc for dev/stage and production ;
            if (System.Configuration.ConfigurationManager.AppSettings["Catalogenvironment"] != null && System.Configuration.ConfigurationManager.AppSettings["Catalogenvironment"] != string.Empty)
            {
                string env = System.Configuration.ConfigurationManager.AppSettings["Catalogenvironment"].ToString();
                storedproc = "uasp_Select_CatalogItems_" + env;
            }
            /*Database database = DatabaseFactory.CreateDatabase("ConnectionString - iPhone");
            DbCommand dbCommand = (DbCommand)database.GetStoredProcCommand(storedproc.Trim());
            database.AddInParameter(dbCommand, "@ApplicationId", DbType.Int32, applicationId);
            using (IDataReader dataReader = database.ExecuteReader(dbCommand))
            {
                while (dataReader.Read())
                {
                    if (items == null)
                    {
                        items = new List<MOBItem>();
                    }
                    MOBItem item = new MOBItem
                    {
                        Id = dataReader["Id"].ToString(),
                        CurrentValue = dataReader["CurrentValue"].ToString()
                    };
                    if (ConfigurationManager.AppSettings["ListOfCatalogItemsToReturnHardCodeTrue"] != null)
                    {
                        List<string> listofCatalogToHardCodetoTrue =
                            ConfigurationManager.AppSettings["ListOfCatalogItemsToReturnHardCodeTrue"].ToString().Split('|').ToList(); //EX "10019|10020"
                        var match = listofCatalogToHardCodetoTrue.FirstOrDefault(stringToCheck => stringToCheck.Contains(item.Id));
                        if (match != null && match.Trim() == item.Id.Trim())
                        {
                            item.CurrentValue = "1";
                        }
                    }
                    if (ConfigurationManager.AppSettings["ListOfCatalogItemsToReturnHardCodeFalse"] != null)
                    {
                        List<string> listofCatalogToHardCodetoTrue =
                            ConfigurationManager.AppSettings["ListOfCatalogItemsToReturnHardCodeFalse"].ToString().Split('|').ToList(); //EX "10018|10022"
                        var match = listofCatalogToHardCodetoTrue.FirstOrDefault(stringToCheck => stringToCheck.Contains(item.Id));
                        if (match != null && match.Trim() == item.Id.Trim())
                        {
                            item.CurrentValue = "0";
                        }
                    }
                    //<add key="ListOfCatalogItemsToChangeValue" value="10000~https://smartphone-preview.united.com/ContinentalRestServices3/api|10012~1"/>
                    if (ConfigurationManager.AppSettings["ListOfCatalogItemsToChangeValue"] != null &&
                        !string.IsNullOrEmpty(ConfigurationManager.AppSettings["ListOfCatalogItemsToChangeValue"]
                            .Split('|').ToList().Find(i => i.Contains(item.Id.Trim()))))
                    {
                        item.CurrentValue = ConfigurationManager.AppSettings["ListOfCatalogItemsToChangeValue"]
                            .Split('|').ToList().Find(i => i.Contains(item.Id.Trim())).Split('~')[1].ToString();
                    }
                    items.Add(item);
                }
            }
            */
            return items;
        }

        public async Task<bool> IsClientCatalogEnabled(int applicationId, string[] clientCatalogIds)
        {
            return true;

            var catalogItems = await GetCatalogItems(applicationId);
            if (catalogItems != null)
            {
                switch (applicationId)
                {
                    case 1:
                        if (catalogItems.Exists(c => c.Id == clientCatalogIds[0]))
                        {
                            return catalogItems.Where(c => c.Id == clientCatalogIds[0]).FirstOrDefault().CurrentValue.Equals("1");
                        }
                        break;
                    case 2:
                        if (catalogItems.Exists(c => c.Id == clientCatalogIds[1]))
                        {
                            return catalogItems.Where(c => c.Id == clientCatalogIds[1]).FirstOrDefault().CurrentValue.Equals("1");
                        }
                        break;
                    default:
                        return false;

                }
            }
            return false;

        }
    }
}

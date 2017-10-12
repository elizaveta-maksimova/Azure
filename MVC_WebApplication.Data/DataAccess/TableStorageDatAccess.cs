using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MVC_WebApplication.Data.Model;

namespace MVC_WebApplication.Data.DataAccess
{
    public class TableStorageDatAccess
    {
        private const string ConnectionStringSettingName = "StorageConnectionString";
        private const string TableName = "Products";

        public void TableLinqOperations()
        {
            CloudTable table = GetTableReference();

            var employees = table.CreateQuery<EmployeeEntity>()
                .Where(e => e.PartitionKey == "Marketing")
                .Select(e => new { e.PartitionKey, e.Name })
                .Take(1000)
                .ToList();

            var employees1 = table.CreateQuery<EmployeeEntity>()
                .Select(e => new { e.PartitionKey, e.Name })
                .Where(e => e.Name == "Linda Lawson")
                .ToList();

            var employees2 = table.CreateQuery<EmployeeEntity>()
                .Take(1000)
                .ToList();

            var employees3 = table.CreateQuery<EmployeeEntity>()
               .AsQueryable()
               .Take(1000)
               .ToList();

            var employees4 = table.CreateQuery<EmployeeEntity>()
               .AsEnumerable()
               .Take(1000)
               .ToList();

            TableQuery<EmployeeEntity> employees5 =
                    table.CreateQuery<EmployeeEntity>();

            List<EmployeeEntity> employeesList = employees5
                .Take(1000)
                .ToList();

            List<EmployeeEntity> employees6 = table.CreateQuery<EmployeeEntity>()
                .Where(e => e.PartitionKey.ToLower() == "marketing")
                .ToList();

            List<EmployeeEntity> employees7 = table.CreateQuery<EmployeeEntity>()
               .Where(e => e.PartitionKey.Length >= 5 &&
                           e.Name.ToLower() == "Peter Peterson")
               .ToList();

            var employees8 = table.CreateQuery<EmployeeEntity>()
               .Where(e => (Gender)e.Gender == Gender.Male)
               .ToList();

            var employees9 = table.CreateQuery<EmployeeEntity>()
               .Where(e => e.Name.Equals("Kris King"))
               .Select(e => new { e.Name, e.RowKey })
               .ToList();

            var employees10 = table.CreateQuery<EmployeeEntity>()
               .Where(e => e.PartitionKey.Equals("Sales") || e.PartitionKey.Equals("Marketing"))
               .Select(e => new { e.Name, e.Gender })
               .ToList();

            var employees11 = table.CreateQuery<EmployeeEntity>()
                .Count(e => e.PartitionKey.Equals("Sales"));
        }

        public CloudTable GetTableReference()
        {
            string connectionString = CloudConfigurationManager.GetSetting(ConnectionStringSettingName);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable container = tableClient.GetTableReference(TableName);

            container.CreateIfNotExists();

            return container;
        } 
    }
}

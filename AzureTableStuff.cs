using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApi.Models;

namespace TodoApi
{

    public class AzureTableStuff
    {
        static CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString("DefaultEndpointsProtocol=https;AccountName=pikachuischeese;AccountKey=SvONmCD2Ji0rmPxKUV4PjrXIzALF6hlgGo5sKweeDTohCoXrhTFMqUFpTw5PLD5zgjrHTFF81BSFGOdIQiu89g==;TableEndpoint=https://pikachuischeese.table.cosmos.azure.com:443/;");
        static CloudTable TodoList = CreateTable("TodoList");

        public static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }
        public static CloudTable CreateTable(string tableName)
        {
            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            // Create a table client for interacting with the table service 
            CloudTable table = tableClient.GetTableReference(tableName);
            return table;
        }
        public static async Task<ActionResult<TodoItem>> InsertOrMergeEntityAsync(TodoItem item)
        {
            TodoItem entity = new TodoItem(item.Iid, item.Name);
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            try
            {
                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);
                Console.WriteLine("we made it!");
                // Execute the operation.
                TableResult result = await TodoList.ExecuteAsync(insertOrMergeOperation);
                TodoItem insertedItem = result.Result as TodoItem;

                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                }

                return entity;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }
        public static async Task<TodoItem> RetrieveEntityUsingPointQueryAsync(CloudTable table, string partitionKey, string rowKey) {
            TableOperation retrieveOperation = TableOperation.Retrieve<TodoItem>(partitionKey, rowKey);
            TableResult result = await table.ExecuteAsync(retrieveOperation);
            TodoItem item = result.Result as TodoItem;
            return item;
        }
        public static async Task<Boolean> DeleteEntityAsync(long id)
        {
            TodoItem deleteEntity = await RetrieveEntityUsingPointQueryAsync(TodoList, "TodoList", "" + id);
            if (deleteEntity == null)
            {
                return false;
            }
            TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
            TableResult result = await TodoList.ExecuteAsync(deleteOperation);
            return true;
        }
        public static ActionResult<IEnumerable<TodoItem>> GetAllEntities()
        {
            return new ActionResult<IEnumerable<TodoItem>> (TodoList.ExecuteQuery(new TableQuery<TodoItem>()).ToList());
        }
    }
}

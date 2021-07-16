namespace TodoApi.Models
{
    using Microsoft.Azure.Cosmos.Table;
    public class TodoItem : TableEntity
    {
        public TodoItem() { 
        }
        public TodoItem(long Id, string name) {
            RowKey = ""+Id;
            PartitionKey = "TodoList";
            this.Iid = Id;
            this.Name = name;
        }
        public long Iid { get; set; }
        public string Name { get; set; }
    }
}
namespace AspNetCoreTodo.Models
{
    public class TodoViewModel
    {
        public TodoViewModel()
        {
            // TodoItem item1 = new TodoItem();
            // item1.Title= "Aprendiendo algo";
            // TodoItem item2 = new TodoItem();
            // item2.Title = "Escribir un paper sobre AI";

            // Items = new TodoItem[]{item1,item2};
        }
        
        public TodoItem[] Items { get; set; }
    }
}


namespace DataAccess.CustomModel
{
    public class RequestDTO<T> where T : class
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchName { get; set; }
        //public int ColumnNum { get; set; }
        //public string? Order { get; set; }

        public required T Model { get; set; }
    }
}

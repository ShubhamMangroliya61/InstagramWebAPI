
namespace DataAccess.CustomModel
{
    public class PaginationResponceModel<T> where T : class 
    {
        public int Totalrecord { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public int RequirdPage { get; set; }

        //public string? Order { get ; set; }

        //public int ColumnNum { get; set; }

        public required List<T> Record  { get; set; }
    }
}

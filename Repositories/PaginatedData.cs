namespace QuilvianSystemBackendDev.Repositories
{
    public class PaginatedData<T>
    {
        public IEnumerable<T> Rows { get; set; }
        public int TotalRows { get; set; }
        public int CurrentPage { get; set; }
        public int PerPage { get; set; }
        public int TotalPages { get; set; }
    }
}

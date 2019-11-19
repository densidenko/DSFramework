namespace DSFramework.Data.Contracts
{
    public class BulkWriteResult<TIdentity>
    {
        public BulkWriteResultItem<TIdentity>[] Items { get; set; } = new BulkWriteResultItem<TIdentity>[0];
    }

    public class BulkWriteResultItem<TIdentity>
    {
        public TIdentity EntityId { get; set; }
        public bool IsOk { get; set; }
        public string Error { get; set; }
    }
}
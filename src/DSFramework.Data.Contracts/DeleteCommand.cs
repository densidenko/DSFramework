namespace DSFramework.Data.Contracts
{
    public class DeleteCommand<TIdentity>
    {
        public TIdentity EntityId { get; set; }

        public string Application { get; set; }

        public string User { get; set; }

        public string OperationId { get; set; }
    }
}
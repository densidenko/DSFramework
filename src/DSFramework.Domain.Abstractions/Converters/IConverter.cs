namespace DSFramework.Domain.Abstractions.Converters
{
    public interface IConverter<T1, T2>
    {
        T2 Convert(T1 source);
        T1 Convert(T2 source);
    }
}
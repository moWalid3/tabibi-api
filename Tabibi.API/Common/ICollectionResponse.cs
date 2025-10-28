namespace Tabibi.API.Common
{
    public interface ICollectionResponse<T>
    {
        List<T> Items { get; init; }
    }
}

namespace Tabibi.API.Common.Sorting
{
    public sealed class SortMappingDefinition<TSource, TDestination> : ISortMappingDefinition
    {
        public required SortMapping[] Mappings { get; init; }
    }
}

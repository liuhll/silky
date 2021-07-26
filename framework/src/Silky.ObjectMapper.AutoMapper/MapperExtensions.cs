namespace Silky.ObjectMapper.AutoMapper
{
    public static class MapperExtensions
    {
        public static T MapTo<T>(this object obj) where T : class
        {
            return AutoMapperConfiguration.Mapper.Map<T>(obj);
        }

        public static TDestination MapTo<TSource, TDestination>(this TSource obj, TDestination entity) where TSource : class where TDestination : class
        {
            return AutoMapperConfiguration.Mapper.Map<TSource, TDestination>(obj, entity);
        }
    }
}
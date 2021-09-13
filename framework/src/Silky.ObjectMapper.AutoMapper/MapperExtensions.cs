using AutoMapper;
using Silky.Core;

namespace Silky.ObjectMapper.AutoMapper
{
    public static class MapperExtensions
    {
        public static T Adapt<T>(this object obj) where T : class
        {
            return EngineContext.Current.Resolve<IMapper>().Map<T>(obj);
        }

        public static TDestination Adapt<TSource, TDestination>(this TSource obj, TDestination entity)
            where TSource : class where TDestination : class
        {
            return EngineContext.Current.Resolve<IMapper>().Map<TSource, TDestination>(obj, entity);
        }
    }
}
namespace Silky.Core
{
    public class Singleton<T> : BaseSingleton
    {
        private static T instance;

        /// <summary>
        /// 指定类型T的单例
        /// </summary>
        public static T Instance
        {
            get => instance;
            set
            {
                instance = value;
                AllSingletons[typeof(T)] = value;
            }
        }
    }
}
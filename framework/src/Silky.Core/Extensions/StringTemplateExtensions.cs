using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Silky.Core.Extensions
{
    public static class StringTemplateExtensions
    {
        /// <summary>
        /// 模板正则表达式
        /// </summary>
        private const string commonTemplatePattern = @"\{(?<p>.+?)\}";

        /// <summary>
        /// 读取配置模板正则表达式
        /// </summary>
        private const string configTemplatePattern = @"\#\((?<p>.*?)\)";
        
        
        /// <summary>
        /// 从配置中渲染字符串模板
        /// </summary>
        /// <param name="template"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string Render(this string template, bool encode = false)
        {
            if (template == null) return default;

            // 判断字符串是否包含模板
            if (!Regex.IsMatch(template, configTemplatePattern)) return template;

            // 获取所有匹配的模板
            var templateValues = Regex.Matches(template, configTemplatePattern)
                .Select(u => new
                {
                    Template = u.Groups["p"].Value,
                    Value = EngineContext.Current.Configuration[u.Groups["p"].Value]
                });

            // 循环替换模板
            foreach (var item in templateValues)
            {
                template = template.Replace($"#({item.Template})",
                    encode ? Uri.EscapeDataString(item.Value?.ToString() ?? string.Empty) : item.Value?.ToString());
            }

            return template;
        }

        private static object ResolveTemplateValue(string template, object data)
        {
            // 根据 . 分割模板
            var propertyCrumbs = template.Split('.', StringSplitOptions.RemoveEmptyEntries);
            return GetValue(propertyCrumbs, data);

            // 静态本地函数
            static object GetValue(string[] propertyCrumbs, object data)
            {
                if (data == null || propertyCrumbs == null || propertyCrumbs.Length <= 1) return data;
                var dataType = data.GetType();

                // 如果是基元类型则直接返回
                if (dataType.IsRichPrimitive()) return data;
                object value = null;

                // 递归获取下一级模板值
                for (var i = 1; i < propertyCrumbs.Length; i++)
                {
                    var propery = dataType.GetProperty(propertyCrumbs[i]);
                    if (propery == null) break;

                    value = propery.GetValue(data);
                    if (i + 1 < propertyCrumbs.Length)
                    {
                        value = GetValue(propertyCrumbs.Skip(i).ToArray(), value);
                    }
                    else break;
                }

                return value;
            }
        }
    }
}
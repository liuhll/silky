using System;
using System.Linq;
using System.Text.RegularExpressions;
using Lms.Core.Exceptions;

namespace Lms.Rpc.Routing.Template
{
    public static class TemplateSegmentHelper
    {
        private const string isVariableReg = @"\{(.*?)\}";
        private const string segmentValReg = @"(?<=\{)[^}]*(?=\})";
        

        private static bool IsVariable(string segmentLine)
        {
            return Regex.IsMatch(segmentLine, isVariableReg);
        }

        public static SegmentType GetSegmentType(string segemnetLine, string serviceName)
        {
            if (!IsVariable(segemnetLine))
            {
                return SegmentType.Literal;
            }

            var segemnetLineVal = Regex.Match(segemnetLine, segmentValReg);
            if (segemnetLineVal == null)
            {
                throw new LmsException("路由格式设置不正常", StatusCode.RouteParseError);
            }

            if (segemnetLineVal.Value.StartsWith("appservice",StringComparison.OrdinalIgnoreCase))
            {
                return SegmentType.AppService;
            }

            if (serviceName.EndsWith(segemnetLine))
            {
                return SegmentType.AppService;
            }

            if (segemnetLine.Contains("="))
            {
                var appServiceName = segemnetLineVal.Value.Split("=")[0];
                if (!serviceName.EndsWith(appServiceName))
                {
                    throw new LmsException("指定的服务应用路由段不正常", StatusCode.RouteParseError);
                }
                return SegmentType.AppService;
            }

            return SegmentType.Path;
        }

        public static string GetSegmentVal(string segemnetLine)
        {
            if (!IsVariable(segemnetLine))
            {
                return segemnetLine;
            }
            var segemnetLineVal = Regex.Match(segemnetLine, segmentValReg);
            return segemnetLineVal.Value;
        }
    }
}
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Silky.Core.Exceptions;

namespace Silky.Rpc.Routing.Template
{
    public static class TemplateSegmentHelper
    {
        private const string isVariableReg = @"\{(.*?)\}";
        private const string segmentValReg = @"(?<=\{)[^}]*(?=\})";


        public static bool IsVariable(string segmentLine)
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
                throw new SilkyException("Incorrect routing format", StatusCode.RouteParseError);
            }

            if (segemnetLineVal.Value.StartsWith("appservice", StringComparison.OrdinalIgnoreCase))
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
                    throw new SilkyException("The specified service application route segment is incorrect",
                        StatusCode.RouteParseError);
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

        public static string GetVariableName(string segemnetLine)
        {
            var segmentVal = GetSegmentVal(segemnetLine);
            return segmentVal.Split(":")[0];
        }
    }
}
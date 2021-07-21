using System;

namespace Silky.Core.Utils
{
    public static class TypeUtils
    {
        public static Type GetTypeByString(string type)
        {
            switch (type.ToLower())
            {
                case "bool":
                    return Type.GetType("System.Boolean", true, true);
                case "byte":
                    return Type.GetType("System.Byte", true, true);
                case "sbyte":
                    return Type.GetType("System.SByte", true, true);
                case "char":
                    return Type.GetType("System.Char", true, true);
                case "decimal":
                    return Type.GetType("System.Decimal", true, true);
                case "double":
                    return Type.GetType("System.Double", true, true);
                case "float":
                    return Type.GetType("System.Single", true, true);
                case "int":
                    return Type.GetType("System.Int32", true, true);
                case "uint":
                    return Type.GetType("System.UInt32", true, true);
                case "long":
                    return Type.GetType("System.Int64", true, true);
                case "ulong":
                    return Type.GetType("System.UInt64", true, true);
                case "object":
                    return Type.GetType("System.Object", true, true);
                case "short":
                    return Type.GetType("System.Int16", true, true);
                case "ushort":
                    return Type.GetType("System.UInt16", true, true);
                case "string":
                    return Type.GetType("System.String", true, true);
                case "date":
                case "datetime":
                    return Type.GetType("System.DateTime", true, true);
                case "guid":
                    return Type.GetType("System.Guid", true, true);
                default:
                    return Type.GetType(type, true, true);
            }
        }

        public static bool GetSampleType(string typeStr, out Type convertType)
        {
            var isSampleType = true;
            switch (typeStr.ToLower())
            {
                case "bool":
                    convertType = Type.GetType("System.Boolean", true, true);
                    break;
                case "byte":
                    convertType = Type.GetType("System.Byte", true, true);
                    break;
                case "sbyte":
                    convertType = Type.GetType("System.SByte", true, true);
                    break;
                case "char":
                    convertType = Type.GetType("System.Char", true, true);
                    break;
                case "decimal":
                    convertType = Type.GetType("System.Decimal", true, true);
                    break;
                case "double":
                    convertType = Type.GetType("System.Double", true, true);
                    break;
                case "float":
                    convertType = Type.GetType("System.Single", true, true);
                    break;
                case "int":
                    convertType = Type.GetType("System.Int32", true, true);
                    break;
                case "uint":
                    convertType = Type.GetType("System.UInt32", true, true);
                    break;
                case "long":
                    convertType = Type.GetType("System.Int64", true, true);
                    break;
                case "ulong":
                    convertType = Type.GetType("System.UInt64", true, true);
                    break;
                case "object":
                    convertType = Type.GetType("System.Object", true, true);
                    break;
                case "short":
                    convertType = Type.GetType("System.Int16", true, true);
                    break;
                case "ushort":
                    convertType = Type.GetType("System.UInt16", true, true);
                    break;
                case "string":
                    convertType = Type.GetType("System.String", true, true);
                    break;
                case "date":
                case "datetime":
                    convertType = Type.GetType("System.DateTime", true, true);
                    break;
                case "guid":
                    convertType = Type.GetType("System.Guid", true, true);
                    break;
                default:
                    convertType = null;
                    isSampleType = false;
                    break;
            }

            return isSampleType;
        }
    }
}
using System;
using System.Collections.Generic;
using DapperExtensions.Models;

namespace DapperExtensions.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DapperAttribute : Attribute
    {
        private const int MAX_SIZE = -1;

        public DapperAttribute(DataType type, bool nullable = false, int? size = null, int? scale = null)
        {
            RawType = type;
            Nullable = nullable;
            Size = size;
            Scale = scale;

            ValidateParameters();
        }

        // public method
        public string GetImplementation()
        {
            var result = GetSqlTypeName();
            result += GetSizeSpecification();
            result += " " + GetNullability();
            return result;
        }

        // private methods
        private DataType RawType { get; set; }
        private bool Nullable { get; set; }
        private int? Size { get; set; }
        private int? Scale { get; set; }

        private void ValidateParameters()
        {
            if (DefaultSizes.ContainsKey(RawType))
            {
                if (Size == null && DefaultSizes.TryGetValue(RawType, out var defaultSize))
                {
                    Size = defaultSize;
                }
                else
                {
                    throw new ArgumentException($"{GetSqlTypeName()} requires a size to be specified.");
                }

                ValidateSizeRange();
            }

            if (DefaultPrecisionScale.ContainsKey(RawType))
            {
                if (Size == null && Scale == null)
                {
                    var (precision, scale) = DefaultPrecisionScale[RawType];
                    Size = precision;
                    Scale = scale;
                }
                else if (Size == null)
                {
                    throw new ArgumentException($"{GetSqlTypeName()} requires precision to be specified when scale is provided.");
                }
                else if (Scale == null)
                {
                    Scale = 0; // Default scale to 0 if not specified
                }

                if (Scale > Size)
                {
                    throw new ArgumentException("Scale cannot be greater than precision for decimal types.");
                }
            }
        }

        private void ValidateSizeRange()
        {
            if (Size == MAX_SIZE) return; // MAX is always valid

            switch (RawType)
            {
                case DataType.VarChar:
                case DataType.Char:
                case DataType.Varbinary:
                case DataType.Binary:
                    if (Size > 8000)
                        throw new ArgumentException($"{GetSqlTypeName()} size cannot exceed 8000. Use MAX_SIZE for larger values.");
                    break;
                case DataType.NVarChar:
                case DataType.NChar:
                    if (Size > 4000)
                        throw new ArgumentException($"{GetSqlTypeName()} size cannot exceed 4000. Use MAX_SIZE for larger values.");
                    break;
            }

            if (Size <= 0 && Size != MAX_SIZE)
            {
                throw new ArgumentException("Size must be positive or MAX_SIZE.");
            }
        }

        private string GetSqlTypeName() => RawType.ToString().ToUpper();

        private string GetNullability() => Nullable ? "NULL" : "NOT NULL";

        private string GetSizeSpecification()
        {
            if (DefaultPrecisionScale.ContainsKey(RawType))
            {
                return Scale > 0 ? $"({Size},{Scale})" : $"({Size})";
            }

            if (DefaultSizes.ContainsKey(RawType))
            {
                return Size == MAX_SIZE ? "(MAX)" : $"({Size})";
            }

            return string.Empty;
        }

        // static elements
        private static readonly Dictionary<DataType, int> DefaultSizes = new Dictionary<DataType, int>
        {
            { DataType.VarChar, 255 },
            { DataType.NVarChar, 255 },
            { DataType.Char, 1 },
            { DataType.NChar, 1 },
            { DataType.Varbinary, 255 },
            { DataType.Binary, 1 }
        };

        private static readonly Dictionary<DataType, (int precision, int scale)> DefaultPrecisionScale = new Dictionary<DataType, (int precision, int scale)>
        {
            { DataType.Decimal, (18, 0) },
            { DataType.Numeric, (18, 0) }
        };
    }
}
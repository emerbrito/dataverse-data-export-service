using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.Data.Export.Schema
{
    public static class ColumnDefinitionFactory
    {

        public const string BigIntDataType = "bigint";
        public const string BooleanDataType = "bit";
        public const string DateTimeDataType = "datetime";
        public const string DecimalDataType = "decimal";        
        public const string DoubleDataType = "float";
        public const string IntegerDataType = "int";
        public const string MoneyDataType = "money";
        public const string StringDataType = "nvarchar";                
        public const string UniqueIdentifierDataType = "uniqueidentifier";

        public static ColumnDefinition CreateBigInt(string name)
        {
            var col = new ColumnDefinition
            {
                Name = name,
                MaxLength = 8,
                Precision = 19,
                Scale = 0,
                TypeName = BigIntDataType
            };

            return col;
        }

        public static ColumnDefinition CreateBoolean(string name)
        {
            var col = new ColumnDefinition
            {
                Name = name,
                MaxLength = 1,
                Precision = 1,
                Scale = 0,
                TypeName = BooleanDataType
            };

            return col;
        }

        public static ColumnDefinition CreateDateTime(string name)
        {
            var col = new ColumnDefinition
            {
                Name = name,
                MaxLength = 8,
                Precision = 23,
                Scale = 0,
                TypeName = DateTimeDataType
            };

            return col;
        }

        public static ColumnDefinition CreateDecimal(string name, int precision)
        {
            var col = new ColumnDefinition
            {
                Name = name,
                MaxLength = 9,
                Precision = 38,
                Scale = precision,
                TypeName = DecimalDataType
            };

            return col;
        }

        public static ColumnDefinition CreateDouble(string name, int precision)
        {
            var col = new ColumnDefinition
            {
                Name = name,
                MaxLength = 8,
                Precision = 53,
                Scale = precision,
                TypeName = DoubleDataType
            };

            return col;
        }

        public static ColumnDefinition CreateInteger(string name)
        {
            var col = new ColumnDefinition
            {
                Name = name,
                MaxLength = 4,
                Precision = 10,
                Scale = 0,
                TypeName = IntegerDataType
            };

            return col;
        }

        public static ColumnDefinition CreateMemo(string name)
        {
            var col = new ColumnDefinition
            {
                Name = name,
                MaxLength = -1,
                Precision = 0,
                Scale = 0,
                TypeName = StringDataType
            };

            return col;
        }

        public static ColumnDefinition CreateMoney(string name, int precision)
        {
            var col = new ColumnDefinition
            {
                Name = name,
                MaxLength = 17,
                Precision = 19,
                Scale = precision,
                TypeName = MoneyDataType
            };

            return col;
        }

        public static ColumnDefinition CreateString(string name, int maxLength)
        {
            var col = new ColumnDefinition
            {
                Name = name,
                MaxLength = maxLength * 2,
                Precision = 0,
                Scale = 0,
                TypeName = StringDataType
            };

            return col;
        }

        public static ColumnDefinition CreateUniqueIdentifier(string name)
        {
            var col = new ColumnDefinition
            {
                Name = name,
                MaxLength = 16,
                Precision = 0,
                Scale = 0,
                TypeName = UniqueIdentifierDataType
            };

            return col;
        }

    }
}

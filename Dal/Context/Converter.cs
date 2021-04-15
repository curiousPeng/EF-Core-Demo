using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dal.Context
{
    public class BoolToDecimalConverter : ValueConverter<bool?, decimal?>
    {
        public BoolToDecimalConverter([CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                  v => v.HasValue ? Convert.ToDecimal(v.Value) : 0,
                  v => v.HasValue ? Convert.ToBoolean(v.Value) : false,
                  mappingHints)
        {
        }

        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(typeof(bool), typeof(decimal), i => new BoolToDecimalConverter(i.MappingHints));
    }
}

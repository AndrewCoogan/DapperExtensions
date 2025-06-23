using System;

namespace DapperExtensions.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TempTableName : Attribute
    {
        public TempTableName(string nameOverride)
        {
            NameOverride = nameOverride;
        }

        // public method
        public string NameOverride { get; private set; }
    }
}
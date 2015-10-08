using System;

namespace VS.CsvResponse
{
    public class CsvColumnAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
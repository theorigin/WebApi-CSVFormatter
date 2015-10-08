using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VS.CsvResponse
{
    public class CsvFormatter : BufferedMediaTypeFormatter
    {
        public Func<object, HttpRequestMessage, object> Selector;

        public string Filename = "export.csv";
         
        static readonly char[] SpecialChars = { ',', '\n', '\r', '"' };

        private readonly HttpRequestMessage _request;

        private readonly string _fields;

        private CsvFormatter(HttpRequestMessage request) : this()
        {
            if (request == null) return;

            _request = request;
            _fields = HttpUtility.ParseQueryString(_request.RequestUri.Query)["fields"] ?? "*";
        }

        public CsvFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/csv"));
            this.AddQueryStringMapping("accept", "text-csv", "text/csv");
        }

        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            return new CsvFormatter(request)
            {
                Selector = Selector,
                Filename = Filename
            };
        }

        public override bool CanWriteType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return true;
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
        {
            using (var writer = new StreamWriter(writeStream))
            {
                var obj = ApplyFunc(value, _request);
                var objType = obj.GetType().GetGenericArguments().First();
                var dt = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(obj), (typeof(DataTable)));                
                var cols = GetColumnNames(objType);               
                var columnNames = cols.Select(column => "\"" + column.Value.Replace("\"", "\"\"") + "\"").ToArray();

                writer.WriteLine(string.Join(",", columnNames));

                foreach (var fields in from DataRow row in dt.Rows select cols.Select(column => Escape(row[column.Key])).ToList())
                    writer.WriteLine(string.Join(",", fields));
            }
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.Add("Content-Disposition", "attachment; filename=" + Filename);
        }

        private Dictionary<string, string> GetColumnNames(Type t)
        {
            var columnNames = new Dictionary<string, string>();
            var type = typeof(CsvColumnAttribute);

            foreach (var propertyInfo in t.GetProperties().Where(propertyInfo => propertyInfo != null && _fields.IndexOf(propertyInfo.Name, StringComparison.OrdinalIgnoreCase) >= 0 || _fields == "*"))
            {
                if (Attribute.IsDefined(propertyInfo, type))
                {
                    var attributeInstance = Attribute.GetCustomAttribute(propertyInfo, type);
                    if (attributeInstance != null)
                    {
                        foreach (var info in type.GetProperties().Where(info => info.CanRead && string.Compare(info.Name, "name", StringComparison.InvariantCultureIgnoreCase) == 0))
                            columnNames.Add(propertyInfo.Name, info.GetValue(attributeInstance, null).ToString());                        
                    }
                    else
                        columnNames.Add(propertyInfo.Name, propertyInfo.Name);
                }
                else
                    columnNames.Add(propertyInfo.Name, propertyInfo.Name);
            }
            return columnNames;
        }

        private object ApplyFunc(object value, HttpRequestMessage request)
        {
            return Selector != null ? Selector(value, request) : value;
        }

        private static string Escape(object o)
        {            
            var field = o.ToString();
            return field.IndexOfAny(SpecialChars) != -1 ? $"\"{field.Replace("\"", "\"\"")}\"" : field;
        }
    }
}
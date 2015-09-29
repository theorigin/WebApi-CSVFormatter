using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json;

namespace VS.CsvResponse
{
    public class CsvFormatter : BufferedMediaTypeFormatter
    {
        public Func<object, HttpRequestMessage, object> Selector;
        
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
        }

        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            return new CsvFormatter(request)
            {
                Selector = Selector
            };
        }

        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

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
                var dt = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(ApplyFunc(value, _request)), (typeof(DataTable)));

                var cols = dt.Columns.Cast<DataColumn>().Where(column => _fields.IndexOf(column.ColumnName, StringComparison.OrdinalIgnoreCase) >= 0 || _fields == "*").Select(x => x.ColumnName).ToList();

                var columnNames = cols.Select(column => "\"" + column.Replace("\"", "\"\"") + "\"").ToArray();

                writer.WriteLine(string.Join(",", columnNames));

                foreach (DataRow row in dt.Rows)
                {
                    var fields = cols.Select(column => Escape(row[column])).ToList();

                    writer.WriteLine(string.Join(",", fields));
                }
            }
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
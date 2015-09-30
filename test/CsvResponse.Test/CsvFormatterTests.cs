using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using NUnit.Framework;
using VS.CsvResponse;

namespace CsvResponse.Test
{
    [TestFixture]
    public class CsvFormatterTests
    {
        [Test]
        public void CorrectDataReturned()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost?fields=Id,Name(");
            var value = GetData().Take(1);

            // Act
            var result = Test(request, value);

            Assert.That(result, Is.EqualTo("\"Id\",\"Name\"\r\n1,Product 1\r\n"));
        }
        
        [Test]
        public void SpecialCharsAreEscaped()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost?fields=Id,Name(");
            var value = GetData().Skip(1).Take(1);

            // Act
            var result = Test(request, value);

            Assert.That(result, Is.EqualTo("\"Id\",\"Name\"\r\n2,\"Product,2 - \"\"It's great\"\"\"\r\n"));
        }

        [Test]
        public void FuncIsUsed()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost?fields=Id,Name(");
            var value = new Holder
            {
                Lines = new List<Line> { new Line { Id = 1, Name = "Line1" } },
                Totals = new Totals { Total1 = 1, Total2 = 2 }
            };

            Func<object, HttpRequestMessage, object> func = (o, h) => ((Holder) o).Lines;

            // Act
            var result = Test(request, value, func);

            Assert.That(result, Is.EqualTo("\"Id\",\"Name\"\r\n1,Line1\r\n"));
        }

        [Test]
        public void AllFieldsReturned()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost?fields=*");
            var value = GetData().Take(1);
            var expectedDate = new DateTime(2015, 09, 28); // Allows for culture differences on servers

            // Act
            var result = Test(request, value);

            Assert.That(result, Is.EqualTo("\"Id\",\"Name\",\"Price\",\"StockQuantity\",\"LastOrderDate\"\r\n1,Product 1,1.23,123," + expectedDate + "\r\n"));
        }

        [Test]
        public void CanReadTypeThrowsException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new CsvFormatter().CanWriteType(null));

            Assert.That(ex.ParamName, Is.EqualTo("type"));
        }

        [Test]
        public void CanReadTypeReturnsTrue()
        {
            Assert.IsTrue(new CsvFormatter().CanWriteType(typeof(Product)));
        }

        [Test]
        public void CanReadTypeReturnsFalse()
        {
            Assert.IsFalse(new CsvFormatter().CanReadType(null));    
        }

        private static string Test<T>(HttpRequestMessage request, T value, Func<object, HttpRequestMessage, object> func = null)
        {
            //var func = Func<object, object> func = (a) => { return null; };
            
            var formatter = new CsvFormatter { Selector = func }.GetPerRequestFormatterInstance(null, request, null);
   
            using (var memoryStream = new MemoryStream())
            {
                formatter.WriteToStreamAsync(typeof(T), value, memoryStream, null, null);

                memoryStream.Flush();
                memoryStream.Position = 0;

                var sr = new StreamReader(memoryStream);
                return sr.ReadToEnd();
            }
        }
        
        private static IEnumerable<Product> GetData()
        {
            yield return new Product
            {
                Id = 1,
                Name = "Product 1"              ,
                LastOrderDate = new DateTime(2015, 09, 28),
                Price = (decimal)1.23,
                StockQuantity = 123
            };

            yield return new Product
            {
                Id = 2,
                Name = "Product,2 - \"It's great\"",
                LastOrderDate = new DateTime(2015, 09, 28),
                Price = (decimal)1.23,
                StockQuantity = 123
            };

            yield return new Product
            {
                Id = 3,
                Name = null,
                LastOrderDate = new DateTime(2015, 09, 28),
                Price = (decimal)1.23,
                StockQuantity = 123
            };
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public DateTime LastOrderDate { get; set; }
    }

    public class Holder
    {
        public List<Line> Lines { get; set; }
        public Totals Totals { get; set; }
    }

    public class Line
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Totals
    {
        public int Total1 { get; set; }
        public int Total2 { get; set; }
    }
}

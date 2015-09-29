[![andyrobinson MyGet Build Status](https://www.myget.org/BuildSource/Badge/andyrobinson?identifier=9522c31d-4062-4dc6-b36a-593de3a39d87)](https://www.myget.org/)

# WebApi-CSVFormatter
Adds CSV formatted response support in ASP.NET Web API

##Usage of partial response
Register the ```CsvFormatter``` in ```Register``` (in WebApiConfig.cs):

```
config.Formatters.Add(new CsvFormatter());
```

Now if an ```Accept``` header with a value of ```text/csv``` is supplied the response will be automatically formatted as CSV before being returned

*I've only used this with simple objects (not nested) so be warned!*

##Delimiters

By default a comma (,) will be used as a delimiter. I plan to add support for any delimiter in the next release

##Reserved characters (commas, double quotes, newlines)

If these characters are encountered they will be wrapped within double quotes ("")

##```fields``` parameter

The ```fields``` parameter allows only specific fields to be returned in the response.

The following rules explain the supported syntax for the ```fields``` parameter value:

* Use a comma-separated list (```fields=a,b```) to select multiple fields.
* Use an asterisk (```fields=*```) as a wildcard to identify all fields.

##Examples

Given an object like this

```csharp
class Product {
  public int Id {get; set;}
  public string Name {get; set;}
  public string Barcode {get; set;}
  public decimal Cost {get; set;}
}
````

Will give the following output...

https://myapi.mycompany.com/products

```
"Id", "Name", "Barcode", "Cost"
1,Banana,08765412,0.45
2,Apple,256895,0.75
3,Orange,895698,0.60
```

https://myapi.mycompany.com/products?fields=id,name

```
"Id", "Name"
1,Banana
2,Apple
3,Orange
```

https://myapi.mycompany.com/products?fields=id,name,cost

```
"Id", "Name", "Cost"
1,Banana,0.45
2,Apple,0.75
3,Orange,0.60
```

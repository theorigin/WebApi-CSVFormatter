[![andyrobinson MyGet Build Status](https://www.myget.org/BuildSource/Badge/andyrobinson?identifier=9522c31d-4062-4dc6-b36a-593de3a39d87)](https://www.myget.org/)

# WebApi-CSVFormatter
Adds CSV formatted response support in ASP.NET Web API

##Usage of partial response
Register the ```CsvFormatter``` in ```Register``` (in WebApiConfig.cs):

```
config.Formatters.Add(new CsvFormatter());
```

or to specify a ```Func```

```
config.Formatters.Add(new CsvFormatter { Selector = func  } );
```

Now if an ```Accept``` header with a value of ```text/csv``` is supplied the response will be automatically formatted as CSV before being returned

##Complex object support
CsvReponse accepts a ```Func``` to control which object is used as the source when a complex object is used, see examples below.

##Delimiters

Well it's a comma (,) as in **Comma** Separated Values :-)

##Reserved characters (commas, double quotes, newlines)

If these characters are encountered they will be wrapped within double quotes ("")

##```fields``` parameter

The ```fields``` parameter controler which fields are returned in the response.

The following rules explain the supported syntax for the ```fields``` parameter value:

* ```fields=id,name``` to select multiple fields
* ```fields=*``` to select all fields

If ```fields=*``` is omitted all fields are returned

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

Given an object like this

```csharp
class Customer {
  public int Id {get; set;}
  public string Name {get; set;}
  public List<Address> Addresses {get; set;}
}

class Address {
  public string Street {get; set;}
  public string Town {get; set;}
  public string County {get; set;}
  public string Postcode {get; set;}
}
````
and a ```Func``` like this

```csharp
Func<object, HttpRequestMessage, object> func = (o, h) => ((Customer)o).Addresses;
````

https://myapi.mycompany.com/customers

```
"Street", "Town", "County", "Postcode"
Davigdor Road,Hove,East Sussex,BN31RE
```

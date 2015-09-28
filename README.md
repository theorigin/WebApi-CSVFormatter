[![andyrobinson MyGet Build Status](https://www.myget.org/BuildSource/Badge/andyrobinson?identifier=9522c31d-4062-4dc6-b36a-593de3a39d87)](https://www.myget.org/)

# WebApi-CSVFormatter
Allows CSV formatted response support in ASP.NET Web API

##Usage of partial response
Register the ```CsvFormatter``` in ```Register``` (in WebApiConfig.cs):

```
config.Formatters.Add(new CsvFormatter());
```

Now if an ```Accept``` header with a value of ```text/csv``` is supplied the response will be automatically formatted as CSV before being returned


##```fields``` parameter

The ```fields``` parameter allows only specific fields to be returned in the response.

The following rules explain the supported syntax for the ```fields``` parameter value:

* Use a comma-separated list (```fields=a,b```) to select multiple fields.
* Use an asterisk (```fields=*```) as a wildcard to identify all fields.

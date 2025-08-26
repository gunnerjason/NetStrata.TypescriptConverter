namespace NetStrata.TypescriptConvert;

public class Person
{
    public string Name { get; set; }
    public string? DriverLicence { get; set; }
    public List<string>? PreviousName { get; set; }
    public int? Age { get; set; }
    public Address? HomeAddress { get; set; }
    public List<Address> PostalAddresses { get; set; }
}

public class Address
{
    public string StreetName { get; set; }
    public string Suberb { get; set; }
    public string State { get; set; }
    public int Postcode { get; set; }
}

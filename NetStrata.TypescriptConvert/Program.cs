using NetStrata.TypescriptConvert;

string textInput = @"public class PersonDto
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string? Gender { get; set; }
    public long? DriverLicenceNumber { get; set; }
    public Address? HomeAddress { get; set; }
    public List<Address> Addresses { get; set; }
    public class Address
    {
        public int StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string Suburb { get; set; }
        public int PostCode { get; set; }
    }
}";

try
{
    var tsConverter = new CSharpToTypescriptStringConverter();
    var output = tsConverter.Convert(textInput);
    
    Console.WriteLine("TSClass after convert:\n");
    Console.WriteLine(output);
}
catch (Exception ex)
{
    Console.WriteLine($"Convert c# class into Typescript class into error [{ex.Message}]");
}

using NetStrata.TypescriptConvert;

try
{
    var tsConverter = new TsConverter();
    tsConverter.Convert("Person");
}
catch (Exception ex)
{
    Console.WriteLine($"Convert c# class into Typescript class into error [{ex.Message}]");
}

using System.Text;
using System.Text.RegularExpressions;

namespace NetStrata.TypescriptConvert;

public partial class CSharpToTypescriptStringConverter
{
    [GeneratedRegex(@"public class (\w+)")]
    private static partial Regex ClassMatchRegex();

    [GeneratedRegex(@"public ([\w<>?]+) (\w+) \{\s*get;\s*set;\s*\}")]
    private static partial Regex PropertyMatchRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceMatchRegex();

    [GeneratedRegex(@"^List<(.+)>$")]
    private static partial Regex ListMatchRegex();

    public string Convert(string csharpCode)
    {
        if (string.IsNullOrWhiteSpace(csharpCode))
        {
            throw new ArgumentException("C# code cannot be null or empty", nameof(csharpCode));
        }

        var classes = ParseCSharpClasses(csharpCode);
        return GenerateTsOutput(classes);
    }

    private static List<ClassModel> ParseCSharpClasses(string csharpCode)
    {
        var classes = new List<ClassModel>();
        
        // Remove all whitespace/tab from each line
        var normalizedCode = NormaliseWhitespace(csharpCode);
        
        // Stack to store the class parent first and then child, but always pop out child class first
        var classStack = new Stack<ClassModel>();
        var classLines = normalizedCode.Split('\n');
        
        foreach (var trimmedLine in classLines)
        {
            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                continue;
            }

            // Check for class start
            if (trimmedLine.StartsWith("public class"))
            {
                var classNameMatch = ClassMatchRegex().Match(trimmedLine);
                if (classNameMatch.Success)
                {
                    var newClass = new ClassModel 
                    { 
                        Name = classNameMatch.Groups[1].Value 
                    };
                    classStack.Push(newClass);
                }
                continue;
            }

            // Check for class end
            if (trimmedLine == "}")
            {
                if (classStack.Count > 0)
                {
                    var completedClass = classStack.Pop();
                    classes.Add(completedClass);
                }
                continue;
            }

            // Check for property in current class
            if (classStack.Count > 0 && trimmedLine.StartsWith("public "))
            {
                var matchedProperty = PropertyMatchRegex().Match(trimmedLine);
                if (matchedProperty.Success)
                {
                    var propertyType = matchedProperty.Groups[1].Value.Trim();
                    var propertyName = matchedProperty.Groups[2].Value.Trim();

                    if (propertyType != "class")
                    {
                        var currentClass = classStack.Peek();
                        currentClass.Properties.Add(new PropertyModel
                        {
                            Name = propertyName,
                            Type = propertyType
                        });
                    }
                }
            }
        }

        // Swap order for parent & child class
        classes.Reverse();
        return classes;
    }

    private static string NormaliseWhitespace(string input)
    {
        var lines = input.Split('\n');
        var trimmedLines = lines.Select(line => WhitespaceMatchRegex().Replace(line.Trim(), " "));
        return string.Join("\n", trimmedLines);
    }

    private string GenerateTsOutput(List<ClassModel> classes)
    {
        var sb = new StringBuilder();
        
        foreach (var classDef in classes)
        {
            sb.AppendLine($"export interface {classDef.Name} {{");
            
            foreach (var property in classDef.Properties)
            {
                var tsType = MapToTsType(property.Type);
                var tsPropertyName = TransformToTypescriptVarCase(property.Name);
                // Flip question mark to property name per requirement
                if (tsType.EndsWith("?"))
                {
                    tsType = tsType[..^1];
                    tsPropertyName += "?";
                }
                sb.AppendLine($"    {tsPropertyName}: {tsType};");
            }
            
            sb.AppendLine("}");
        }

        return sb.ToString().TrimEnd();
    }

    private static string MapToTsType(string csharpType)
    {
        if (csharpType.EndsWith("?"))
        {
            var baseType = csharpType.TrimEnd('?');
            return $"{MapToTsType(baseType)}?";
        }

        var listMatch = ListMatchRegex().Match(csharpType);
        if (listMatch.Success)
        {
            var listInnerType = listMatch.Groups[1].Value;
            return $"{MapToTsType(listInnerType)}[]";
        }

        return csharpType.ToLower() switch
        {
            "string" => "string",
            "int" or "long" => "number",
            _ => csharpType
        };
    }
    private string TransformToTypescriptVarCase(string cSharpVarCase)
    {
        if (string.IsNullOrEmpty(cSharpVarCase))
        {
            return cSharpVarCase;
        }

        return char.ToLower(cSharpVarCase[0]) + cSharpVarCase[1..];
    }

    private class ClassModel
    {
        public string Name { get; init; } = default!;
        public List<PropertyModel> Properties { get; } = [];
    }

    private class PropertyModel
    {
        public string Name { get; init; } = default!;
        public string Type { get; init; } = default!;
    }
}

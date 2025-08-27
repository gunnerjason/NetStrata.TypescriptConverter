using System.Reflection;

namespace NetStrata.TypescriptConvert;

/// <summary>
/// This converter to expect a C# object as input
/// </summary>
public class TsConverter
{
    private class TsModel
    {
        public string PropertyName { get; init; } = default!;
        public string PropertyType { get; init; } = default!;
        public bool PropertyNullable { get; init; }

        public override string ToString() => $"{PropertyName}{(PropertyNullable ? "?" : "")}: {PropertyType};";
    }

    private static readonly Dictionary<Type, string> TypeMap = new()
    {
        { typeof(string), "string" },
        { typeof(long), "number" },
        { typeof(int), "number" },
    };

    /// <summary>
    /// To hold any class type property within the root class 
    /// </summary>
    private readonly Dictionary<string, Type> _childClasses = new();

    private Type ReadInputClass(string inputClassName)
    {
        var assembly = Assembly.LoadFrom("NetStrata.TypescriptConvert.dll");
        var type = assembly.GetType($"NetStrata.TypescriptConvert.{inputClassName}");

        if (type is null)
        {
            throw new ArgumentException("Input class is not found in assembly.");
        }

        return type;
    }

    public void Convert(string inputClassName)
    {
        var rootClassType = ReadInputClass(inputClassName);

        var rootClassTsProperties = GetClassTsProperties(rootClassType);
        PrintTsClass(rootClassType.Name, rootClassTsProperties);
        
        if (_childClasses.Count > 0)
        {
            foreach (var childClass in _childClasses)
            {
                var childClassTsProperties = GetClassTsProperties(childClass.Value);
                PrintTsClass(childClass.Value.Name, rootClassTsProperties);
            }
        }
    }

    private void PrintTsClass(string className, List<TsModel> classTsProperties)
    {
        Console.WriteLine($"export interface {className} {{");
        classTsProperties.ForEach(p => Console.WriteLine("\t"+p));
        Console.WriteLine("}");
    }
    
    /// <summary>
    /// Assumption of child class only has 1 level, no need for recursive process 
    /// </summary>
    private List<TsModel> GetClassTsProperties(Type classType, bool rootLevel = true)
    {
        List<TsModel> classProperties = [];
        foreach (var classProperty in classType.GetProperties())
        {
            var tsModel = new TsModel
            {
                PropertyName = classProperty.Name,
                PropertyType = GetMappedType(classProperty),
                PropertyNullable = IsPropertyNullable(classProperty),
            };
            
            classProperties.Add(tsModel);
        }

        return classProperties;
    }

    private string GetMappedType(PropertyInfo property, bool rootLevel = true)
    {
        var type = property.PropertyType;

        // Nullable<T> trim
        if (Nullable.GetUnderlyingType(type) is Type underlying)
        {
            return MapNonNullableType(underlying);
        }

        // List<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var elementType = type.GetGenericArguments()[0];
            string tsType;
            if (elementType != typeof(string) && elementType.IsClass)
            {
                if (rootLevel)
                {
                    _childClasses.TryAdd(elementType.Name, elementType);
                }
                tsType = $"{elementType.Name}[]";
            }
            else
            {
                tsType = $"{MapNonNullableType(elementType)}[]";
            }
            return tsType;
        }

        if (type != typeof(string) && type.IsClass)
        {
            var tsType = type.Name;
            if (rootLevel)
            {
                _childClasses.TryAdd(type.Name, type);
            }
            return tsType;
        }

        return MapNonNullableType(type);
    }

    private bool IsPropertyNullable(PropertyInfo property)
    {
        var type = property.PropertyType;

        // Test int? long?
        if (Nullable.GetUnderlyingType(type) is not null)
        {
            return true;
        }

        // Test int, long
        if (type.IsValueType)
        {
            return false;
        }

        // Test string, class, List<>
        return IsNullableReferenceType(property);
    }

    /// <summary>
    /// string? type is also needed to be checked by the reference type
    /// </summary>
    private bool IsNullableReferenceType(PropertyInfo property)
    {
        var nullableContextAttr = property.GetMethod?.CustomAttributes
            .Where(x => x.AttributeType.Name == "NullableContextAttribute").Count();
    
        var nullableAttr = property.CustomAttributes
            .FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");

        if (nullableAttr is { ConstructorArguments.Count: > 0 } || nullableContextAttr > 0)
        {
            return true;
        }

        return false;
    }
    
    private string MapNonNullableType(Type type)
    {
        if (TypeMap.TryGetValue(type, out var tsType))
        {
            return tsType;
        }

        throw new InvalidOperationException($"Class property has type [{type.Name}] not convertible by the system.");
    }
}

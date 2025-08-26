### C# → TypeScript Class Converter (Minimal Demo)

This is a minimal demo project that converts C# classes into TypeScript interfaces.

##### Features

- Converts a sample input class (Person) into a TypeScript interface.
- Can be extended to take a class name as a system input argument and generate the corresponding TypeScript interface from the project assembly.
- Handles the following property types:
  - ```Nullable string, int, long```
  - ```List<T> collections```
  - ```Subclass references```

##### Limitations

- Only the first level of nested classes will be converted (no deep recursion).
- Limited to the supported property types listed above.
- Given this is a very simple code snippet for type convert, I didn't make it overkill with SOLID principle to have multiple classes for future scalability.

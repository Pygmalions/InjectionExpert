# Injection Expert

Injection Expert is a dependency injection framework for .NET, 
featuring dynamic code generation for high performance and flexibility. 
It supports constructor and member injection, generic type resolution, and advanced configuration options.

Compared to `Microsoft.Extensions.DependencyInjection`, 
this library offers more flexibility by allowing dynamically adding injections.
This library has good compatibility with `Microsoft.Extensions.DependencyInjection`,
`IInjectionProvider` of this library can be easily adapted to `IServiceProvider`, and vice versa.

## Key Concepts

- **InjectionContainer**: The registry for dependencies. 
Supports singleton and transient lifetimes, generic type mapping, and resolution of injections.
- **ConstructorInjector**: Dynamically generates code to inject dependencies via constructors.
- **MemberInjector**: Dynamically generates code to inject dependencies into fields and properties marked with `[Injection]` attribute.
- **Attributes**: `[Injection]` requires injector to inject fields/properties without `required` keyword, 
  or ignore required members. This attribute can also indicate whether the member needs keyed injection.

## Rules

**Rules for Selecting Members to Inject**

Fields or properties must fulfill all requirements at the same time:
- Is marked with `required` keyword or `[Injection]` attribute when `enabled` parameter is true.
- Is not a literal or init-only field.
- Is not a read-only property.

- **Rules for Selecting Constructors to Inject**

1. Constructors with the lowest number of parameters are prioritized.
2. The first construct that all parameters can be resolved is selected.

## Usage

### Registering and Resolving Dependencies

```csharp
var container = new InjectionContainer()
    .AddSingleton(1)
    .AddSingleton(0.5)
    .AddSingleton("Sample");

// Constructor injection
var succeeded = ConstructorInjector
    .For(typeof(MyClass))
    .TryInject(out var instance, container, out _);

var target = (MyClass?)instance;
if (succeeded && target != null)
{
    Console.WriteLine(target.Text);   // "Sample"
    Console.WriteLine(target.Number); // 1
    Console.WriteLine(target.Value);  // 0.5
}
```

### Member Injection

```csharp
public class MyTarget
{
    // This field will be injected because it is marked with [Injection]
    [Injection] public int NumberField = 0;
    // This field will be injected because it is marked as required
    public required string StringField = "";
    // This field will NOT be injected even it is a required member, because it is marked with [Injection(enabled: false)
    [Injection(false)] public required int IgnoredMember = "";
}

var container = new InjectionContainer()
    .AddSingleton(1)
    .AddSingleton("Sample");

var sample = new MyTarget();
MemberInjector
    .For(typeof(MyTarget))
    .Inject(sample, container);

// sample.NumberField == 1
// sample.StringField == "Sample"
```

### Generic Type Registration

```csharp
container.AddTransient(typeof(IMyGenericInterface<,>), typeof(MyGenericType<,>));
var instance = container.GetInjection<IMyGenericInterface<int, long>>();
// instance is resolved as MyGenericType<long, int>
```

## Features

- High-performance dynamic code generation for injection.
- Supports both constructor and member injection.
- Generic type mapping and resolution.
- Attribute-based configuration for fine control.
- Extensible and suitable for advanced scenarios.

---

For advanced usage and configuration, please refer to the source code and test cases.

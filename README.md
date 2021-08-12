![MIT](https://img.shields.io/badge/license-MIT-blue) [![NuGet](https://img.shields.io/nuget/v/Fluend.ExpressionLanguage)](https://www.nuget.org/packages/Fluend.ExpressionLanguage/)
## Summary
This project aims to be a feature-complete implementation of the Symfony expression language ([link to Symfony documentation](https://symfony.com/doc/current/components/expression_language.html)).
It is not a line-by-line copy of the original implementation, but a real C# implementation of the language. 
For an overview of all capabilities of the expression language, please see the Symfony documentation.
Further details about the C# implementations are found in this readme.

## Installation
You can install the library using NuGet:
```
dotnet add package Fluend.ExpressionLanguage
```
## Usage
Executing a simple expression:
```c#
var expression = "'The answer is: ' ~ (2 ** 6 - 2**4 - 6)";
var result = Expression.Run(expression);

// result.Value is "The answer is: 42"
```

Registering a function and calling it:
```c#
var expression = "(2 ** foo(8)) > 128";
var functions = new ExpressiveFunctionSet();
var foo = new ExpressiveFunction<Func<double, double>>("foo", val => val / 2);
functions.Add(foo);
var result = Expression.Run(expression, functions);

// result.Value is 'true'
```
Please see the [functions and overloading](#functions-and-overloading) section for more information about functions.

Calling a function that takes a variable:
```c#
var expression = "foo(a)";
var foo = new ExpressiveFunction<Func<double, double>>("foo", d => d * 2);
var functions = new ExpressiveFunctionSet();
functions.Add(foo);

var result = Expression.Run(expression, functions, new Dictionary<string, object>
{
    {"a", 42.0d} // Note the use of a double here!
});

// result.Value is 84
```

Evaluating an expression that produces an error:
```c#
var expression = "foo(a)";
var foo = new ExpressiveFunction<Func<double, double>>("foo", d => d * 2);
var functions = new ExpressiveFunctionSet();
functions.Add(foo);

// Oops! Forgot to pass the variable 'a'
var result = Expression.Run(expression, functions, new Dictionary<string, object> { });

// result.Succeeded is 'false'
// result.Error is "The variable 'a' does not exist. Position: 1, a"
```

## The type system
An expression can be a simple as a constant:
 - `3`: will return a `double` containing `3`.
 - `'abc'` will return a `string` containing `abc`.
 - `"abc2"` will return a `string` containing `abc2`.
 
Of course you can use operators to make your expression more useful:
 - `2 + 2` will return a `double` containing `4`.
 - `'The answer is: ' ~ 42` will return a `string` containing `The answer is: 42`.
 
All functions that are exposed to the expression have a signature that is known beforehand. 
This is used to decide on the right overload for example. When evaluating the type (see [determining return types](#determining-the-return-type-of-an-expression-without-executing-it)), variables are always considered as `object?` because their type is decided at runtime.

#### A note on numeric values
All numbers that are found in the expression language are parsed as a double, even integers.
This means that functions that normally expect an integer as an argument (e.g. `Substring`) need to accept a `double` instead, and cast that to an `int`.
This could be solved in the future.

## Functions
All functions are stored as an `ExpressiveFunction`. The actual instantiation uses the generic `ExpressiveFunction<TFunc>` class, where `TFunc` is constrained to a `delegate`.
If, for example, you want to define a function that doubles the numeric value it is given, you can do the following:
```c#
var foo = new ExpressiveFunction<Func<double, double>>("foo", d => d * 2);
```

While not recommended (as functions in expressions generally should not have side effects), it is possible to define a function that does not return anything by using `Action` instead of `Func`:
```c#
var foo = new ExpressiveFunction<Action<double>>("foo", val => Console.WriteLine(val));
```

Note that reflection is used upon construction of the `ExpressiveFunction` object to determine its signature. If you expose the same function to multiple expression evaluations, it is recommended to store the constructed `ExpressiveFunction` object and re-use it.
Functions are always invoked using `DynamicInvoke`.

### Overloading
When you have multiple functions with the same name registered to an expressions, their signatures should differ. Based on the arguments given, a suitable overload is chosen (if available).
For example:
```c#
var foo1 = new ExpressiveFunction<Func<double, double>>("foo", d => d * 2);
var foo1 = new ExpressiveFunction<Func<double, double, double>>("foo", (d1, d2) => d1 * d2);
var functions = new ExpressiveFunctionSet();
functions.Add(foo1);
functions.Add(foo2);
```

If you call `foo(2)` in an expression, the first overload is chosen. `foo(2, 4)` will call the second overload. Note that you cannot overload based on the return type of the function.

### How to structure sets of overloads
While most certainly not mandatory, combining overloads into a function set and merging it into the final set of functions is a good way to keep track of your functions and overloads:
```c#
// A static class containing the string functions that can be
// exposed to an expression.
public static class StringFunctions
{
    // The 'default' case, where only strings are passed.
    private static readonly ExpressiveFunction StartsWithStringString
        = new ExpressiveFunction<Func<string, string, bool>>(
            "startsWith", (subject, find) => subject.StartsWith(find));

    // This overload allows startsWith to be called with a numeric
    // value, which is especially convenient if you use a variable
    // that could be numeric.
    private static readonly ExpressiveFunction StartsWithStringDouble
        = new ExpressiveFunction<Func<string, double, bool>>(
            "startsWith", (subject, find) => subject.StartsWith(
                find.ToString(CultureInfo.InvariantCulture)));

    /// <summary>
    /// Contains the following functions:
    ///  - startsWith(string, string) -> bool
    ///  - startsWith(string, double) -> bool
    /// Usage: startsWith('auto', 'au') -> true
    /// </summary>
    public static readonly ExpressiveFunctionSet StartsWith = new(
        StartsWithStringString,
        StartsWithStringDouble);
}

public class YourClass
{
    public EvaluationResult RunSomeExpression(string expression, IDictionary<string, object?> variables)
    {
        // Note: you could save this in a private static readonly variable and
        // re-use it for every invoked expression (as some sort of "standard function library").
        ExpressiveFunctionSet functions = new();
        
        // Add all overloads of the 'startsWith' function to
        // the final function list.
        functions.Merge(StringFunctions.StartsWith);
        
        return Expression.Run(expression, functions, variables);
    }
}
```

## Objects
You are able to expose objects with properties and methods to the expression language. 
Because many use cases for expressions involve user input, the choice was made that it should be explicit which properties and methods are reachable from the expression language.

You can define and expose an object as follows:
```c#
public class SomeObject : ExpressiveObject
{
    [Expressive]
    public int SomeValue { get; } = 42;

    public double SomeHiddenValue { get; } = 1.0;
        
    [Expressive]
    public string GetName()
    {
        return "SomeName";
    }
}
```

There are two key components:
 1. The object needs to inherit from `ExpressiveObject`.
 2. The exposed property or method needs to have the `Expressive` attribute attached.

You can then use it as follows:
```c#
var expression = "object.SomeValue";
var someObject = new SomeObject();
var result = Expression.Run(expression, new ExpressiveFunctionSet(), new Dictionary<string, object>
{
    {"object", someObject}
});

// result.Value is '42'
```

Calling methods works in the same way:
```c#
var expression = "object.GetName()";
var someObject = new SomeObject();
var result = Expression.Run(expression, new ExpressiveFunctionSet(), new Dictionary<string, object>
{
    {"object", someObject}
});

// result.Value is "SomeName"
```

If an attempt is made to call `SomeHiddenValue`, it will return an error reporting that no such property exists.
```c#
var expression = "object.SomeHiddenValue";
var someObject = new SomeObject();
var result = Expression.Run(expression, new ExpressiveFunctionSet(), new Dictionary<string, object>
{
    {"object", someObject}
});

// result.Succeeded is false
// result.Error is "No property with the name 'SomeHiddenValue' exists on the object."
```

## Advanced
### Advanced usage
Every invocation if the expression language is structured as follows:
```c#
EvaluationResult result = new();         
try
{
    var tokens = new Lexer().Tokenize(expression);
    var parsed = new Parser(functions.GetFunctionNames().ToList())
        .Parse(tokens, variables.Keys.ToList());
    var evaluator = new Evaluator(functions);
    return evaluator.Evaluate(parsed, variables);
}
catch (Exception exception)
{
    result.Succeeded = false;
    result.Error = exception.Message;
}

return result;
```

The code above is the actual implementation of `Expression.Run`. If you want to inspect intermediate values or wish to do detailed configuration on the `evaluator`, you should invoke the expression language in this way instead of using `Expression.Run`.

### Changing the tolerance for floating point equality checks
By default, the evaluator uses a tolerance of `1E-9` when comparing `double` values.
If you wish to change this tolerance, you can use the `evaluator.SetTolerance(newTolerance)` method.

### Changing the timeout for regular expressions
By default, the timeout on regular expressions for the `matches` operator is 250 milliseconds.
If you wish to change this, you can use the `evaluator.SetRegexTimeout(newTimeout)` method.
If you wish to disable the timeout, pass `Timeout.InfiniteTimeSpan` to this method.

### Determining the return type of an expression without executing it
The `TypeResolver` class can be used to determine the type of the result of an expression.
Take this example:
```c#
Type resultType = new TypeResolver().Resolve("3", new Dictionary<string, object?>());
```
Here, `resultType` will contain the type of `double`. This can be done for any expression, even those that include functions or variables. Please note the type resolver can't resolve the type of a variable (as the type of a variable is decided by the runtime arguments); every variable is considered a `object?` by the type resolver.

### What was changed from the original (Symfony) implementation?
#### Tokenizer
The original tokenizer uses a regex-based approach. We have found this implementation to require more time and memory than required. The required CPU-time of the original implementation also seems to grow non-linearly with the length of the expression. Our tokenizer is completely linear and requires less memory.

#### Parser
Most of the original parser is unchanged. A major addition is the "hash" node for hashtables. In the PHP implementation there is no major distinction in the way arrays and hashtables are handled, but C# does require this to decide on the type. 

### Evaluator
The PHP implementation "compiles" the expression to PHP and `eval`'s the result. This implementation walks the AST instead.
In the future, a compilation step could be considered, where the parsed expression is compiled to IL. 
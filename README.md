## Summary
This project aims to be a feature-complete implementation of the Symfony expression language ([link to Symfony documentation](https://symfony.com/doc/current/components/expression_language.html)).
It is not a line-by-line copy of the original implementation, but a real C# implementation of the language.  

## Installation

## Examples

## Functions and overloading

## A note on numeric values
All numbers that are found in the expression language are parsed as a double, even integers.
This means that functions that normally expect an integer as an argument (e.g. `Substring`) need to accept a `double` instead, and cast that to an `int`.
This could be solved in the future.

## Advanced
### Deriving the type of an expression 

### What was changed from the original implementation?
#### Tokenizer
The original tokenizer uses a regex-based approach. We have found this implementation to require more time and memory than required. The required CPU-time of the original implementation also seems to grow non-linearly with the length of the expression. Our tokenizer is completely linear and requires less memory.

#### Parser
Most of the original parser is unchanged. A major addition is the "hash" node for hashtables. In the PHP implementation there is no major distinction in the way arrays and hashtables are handled, but C# does require this to decide on the type. 

### Evaluator
The PHP implementation "compiles" the expression to PHP and `eval`'s the result. This implementation walks the AST instead.
In the future, a compilation step could be considered, where the parsed expression is compiled to IL. 
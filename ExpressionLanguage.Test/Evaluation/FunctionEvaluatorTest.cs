using System;
using System.Collections.Generic;
using System.Linq;
using Fluend.ExpressionLanguage.Evaluation;
using Fluend.ExpressionLanguage.Evaluation.Functions;
using Fluend.ExpressionLanguage.Lexing;
using Fluend.ExpressionLanguage.Parsing;
using FluentAssertions;
using NUnit.Framework;

namespace Fluend.ExpressionLanguage.Test.Evaluation
{
    public class FunctionEvaluatorTest
    {
        [Test]
        public void It_Can_Evaluate_A_Simple_Function_Expression()
        {
            var expression = "foo()";
            var parsed = new Parser(new List<string> {"foo"})
                .Parse(new Lexer().Tokenize(expression));
            
            var functions = new ExpressiveFunctionSet();
            var getAnswerToAllLife =
                new ExpressiveFunction<Func<double>>("foo", () => 42);
            
            functions.Add(getAnswerToAllLife);
            
            var evaluator = new Evaluator(functions);
            var result = evaluator.Evaluate(parsed);

            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(42);
        }
        
        [Test]
        public void It_Can_Evaluate_A_Function_Expression_That_Takes_Parameters()
        {
            var expression = "double(2)";
            var parsed = new Parser(new List<string> {"double"})
                .Parse(new Lexer().Tokenize(expression));
            
            var functions = new ExpressiveFunctionSet();
            var multiply =
                new ExpressiveFunction<Func<double, double>>("double", val => val * 2);
            
            functions.Add(multiply);
            
            var evaluator = new Evaluator(functions);
            var result = evaluator.Evaluate(parsed);

            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(4);
        }
        
        [Test]
        public void It_Can_Evaluate_A_Function_Expression_That_Takes_A_HashTable()
        {
            var expression = "hashItOut({ test: 42 })";
            var parsed = new Parser(new List<string> {"hashItOut"})
                .Parse(new Lexer().Tokenize(expression));
            
            var functions = new ExpressiveFunctionSet();
            var hashItOut = new ExpressiveFunction<Func<Dictionary<string, object>, double>>(
                "hashItOut", table => (double) table["test"]);
            
            functions.Add(hashItOut);
            
            var evaluator = new Evaluator(functions);
            var result = evaluator.Evaluate(parsed);

            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(42);
        }
        
        [Test]
        public void It_Can_Evaluate_An_Overloaded_Function()
        {
            var functions = new ExpressiveFunctionSet();
            var f1 = new ExpressiveFunction<Func<double, double>>("foo", val => 1);
            var f2 = new ExpressiveFunction<Func<string, double>>("foo", val => 2);
            
            functions.Add(f1);
            functions.Add(f2);
            
            var evaluator = new Evaluator(functions);
            var parser = new Parser(functions.GetFunctionNames().ToList());
            
            var expression = "foo(42)";
            var parsed = parser.Parse(new Lexer().Tokenize(expression));
            
            var result = evaluator.Evaluate(parsed);
            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(1);

            expression = "foo('abc')";
            parsed = parser.Parse(new Lexer().Tokenize(expression));

            result = evaluator.Evaluate(parsed);
            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(2);
        }
    }
}
using System;
using System.Collections.Generic;
using Fluend.ExpressionLanguage.Evaluation.Functions;
using FluentAssertions;
using NUnit.Framework;

namespace Fluend.ExpressionLanguage.Test.Evaluation
{
    public class VariableEvaluatorTest
    {
        [Test]
        public void It_Can_Handle_Variables()
        {
            var expression = "a";
            var result = Expression.Run(expression,
                new(),
                new Dictionary<string, object>
                {
                    {"a", 42}
                });

            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(42);
        }
        
        [Test]
        public void It_Can_Handle_Variables_In_Calculations()
        {
            var expression = "a * 2";
            var result = Expression.Run(expression,
                new(),
                new Dictionary<string, object>
                {
                    {"a", 42}
                });

            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(84);
        }

        [Test]
        public void It_Can_Handle_Variables_In_Function_Calls()
        {
            var expression = "foo(a)";
            var function = new ExpressiveFunction<Func<double, double>>(
                "foo", d => d * 2);
            var functions = new ExpressiveFunctionSet();
            functions.Add(function);
            
            var result = Expression.Run(expression,
                functions,
                new Dictionary<string, object>
                {
                    {"a", 42.0d}
                });

            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(84);
        }
        
        [Test]
        public void It_Can_Handle_Variables_In_HashTables()
        {
            var expression = "{a: a}";
            var function = new ExpressiveFunction<Func<double, double>>(
                "foo", d => d * 2);
            var functions = new ExpressiveFunctionSet();
            functions.Add(function);
            
            var result = Expression.Run(expression,
                functions,
                new Dictionary<string, object>
                {
                    {"a", 42.0d}
                });

            result.Succeeded.Should().Be(true);
            var table = (Dictionary<string, object?>) result.Value!; 
            table.Count.Should().Be(1);
            table.ContainsKey("a").Should().BeTrue();
            table["a"].Should().Be(42);
        }

        [Test]
        public void It_Can_Handle_Variables_In_Arrays()
        {
            var expression = "[1, a, a * 2]";
            var function = new ExpressiveFunction<Func<double, double>>(
                "foo", d => d * 2);
            var functions = new ExpressiveFunctionSet();
            functions.Add(function);
            
            var result = Expression.Run(expression,
                functions,
                new Dictionary<string, object>
                {
                    {"a", 42.0d}
                });

            result.Succeeded.Should().Be(true);
            var list = (List<object?>) result.Value!; 
            list.Count.Should().Be(3);
            list[1].Should().Be(42);
        }

        [Test]
        public void It_Can_Handle_Variables_In_Array_Subscripts()
        {
            var expression = "[1, 2, 3][a]";
            var function = new ExpressiveFunction<Func<double, double>>(
                "foo", d => d * 2);
            var functions = new ExpressiveFunctionSet();
            functions.Add(function);
            
            var result = Expression.Run(expression,
                functions,
                new Dictionary<string, object>
                {
                    {"a", 1}
                });

            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(2);
        }

        [Test]
        public void It_Can_Handle_Variables_In_HashTable_Subscripts()
        {
            var expression = "{test: 42}[a]";
            var function = new ExpressiveFunction<Func<double, double>>(
                "foo", d => d * 2);
            var functions = new ExpressiveFunctionSet();
            functions.Add(function);
            
            var result = Expression.Run(expression,
                functions,
                new Dictionary<string, object>
                {
                    {"a", "test"}
                });

            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(42);
        }
        
        [Test]
        public void It_Can_Handle_Variables_In_Numeric_Ranges()
        {
            var expression = "0..a";
            var functions = new ExpressiveFunctionSet();
            var result = Expression.Run(expression,
                functions,
                new Dictionary<string, object>
                {
                    {"a", 10}
                });

            result.Succeeded.Should().Be(true);
            ((List<object?>) result.Value!).Count.Should().Be(10);
        }
        
        [Test]
        public void It_Reports_An_Error_If_A_Defined_Variable_Is_Not_Given()
        {
            var expression = "a";
            
            var result = Expression.Run(expression);

            result.Succeeded.Should().Be(false);
            result.Error.Should().Be("The variable 'a' does not exist. Position: 1, a");
        }
    }
}
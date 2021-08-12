using System;
using System.Collections.Generic;
using Fluend.ExpressionLanguage.Evaluation;
using Fluend.ExpressionLanguage.Evaluation.Functions;
using Fluend.ExpressionLanguage.Parsing;
using Fluend.ExpressionLanguage.Parsing.Nodes;
using FluentAssertions;
using NUnit.Framework;

namespace Fluend.ExpressionLanguage.Test.Evaluation.Functions
{
    public class ExpressiveFunctionTest
    {
        [Test]
        public void It_Resolves_The_Signature_From_The_Given_Callable()
        {
            var function = new ExpressiveFunction<Func<double, int>>(
                "foo", var => (int) var);

            function.Signature.Parameters.Count.Should().Be(1);
            function.Signature.Parameters[0].Type.Should().Be(typeof(double));
            function.Signature.ReturnType.Should().Be(typeof(int));
        }

        [Test]
        public void It_Resolves_The_Signature_From_The_Given_Callable_If_There_Is_No_Return_Type()
        {
            var function = new ExpressiveFunction<Action<double>>(
                "foo", Console.WriteLine);

            function.Signature.Parameters.Count.Should().Be(1);
            function.Signature.Parameters[0].Type.Should().Be(typeof(double));
            function.Signature.ReturnType.Should().Be(typeof(void));
        }

        [Test]
        public void It_Can_Check_Whether_The_Function_Is_Callable_With_A_Given_A_Set_Of_Types()
        {
            var function = new ExpressiveFunction<Action<int, string>>(
                "foo", (i, s) => Console.WriteLine(s + i));

            function.IsCallableWithArguments(new[] {typeof(int), typeof(string)})
                .Should().BeTrue();
            function.IsCallableWithArguments(new[] {typeof(string), typeof(int)})
                .Should().BeFalse();
            function.IsCallableWithArguments(new[] {typeof(int), typeof(string), typeof(int)})
                .Should().BeFalse();
        }

        [Test]
        public void It_Can_Check_Whether_The_Function_Is_Callable_Given_An_Arguments_Node_And_Type_Resolver()
        {
            var function = new ExpressiveFunction<Action<double, string>>(
                "foo", (i, s) => Console.WriteLine(s + i));
            var typeResolver = new TypeResolver();

            var argumentsNode = new ArgumentsNode(new List<Node>
            {
                new ConstantNode(1.0),
                new ConstantNode(""),
            });

            function.IsCallableWithArguments(typeResolver, argumentsNode, new Dictionary<string, object?>())
                .Should().BeTrue();

            argumentsNode = new ArgumentsNode(new List<Node>
            {
                new ConstantNode(1.0),
                new ConstantNode(""),
                new ConstantNode(false)
            });

            function.IsCallableWithArguments(typeResolver, argumentsNode, new Dictionary<string, object?>())
                .Should().BeFalse();
        }
    }
}
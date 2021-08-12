using System;
using Fluend.ExpressionLanguage;
using Fluend.ExpressionLanguage.Evaluation.Functions;
using FluentAssertions;
using NUnit.Framework;

namespace Fluend.ExpressionLanguage.Test.Evaluation
{
    public class EvaluatorTest
    {
        [Test]
        public void It_Can_Evaluate_A_Complex_Expression()
        {
            var expression = "(2 ** foo(8)) > 128";
            var functions = new ExpressiveFunctionSet();
            var someCalculation =
                new ExpressiveFunction<Func<double, double>>("foo", val => val / 2);
            functions.Add(someCalculation);

            var result = Expression.Run(expression, functions);

            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(false);
        }
        
        [Test]
        public void It_Can_Evaluate_A_Complex_String_Expression()
        {
            var expression = "'The answer is: ' ~ (2 ** 6 - 2**4 - 6)";
            var result = Expression.Run(expression);

            result.Succeeded.Should().Be(true);
            result.Value.Should().Be("The answer is: 42");
        }

        [Test]
        public void It_Handles_Very_Long_Numbers()
        {
            var expression =
                "333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333";
            var result = Expression.Run(expression);

            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(double.PositiveInfinity);
        }
    }
}
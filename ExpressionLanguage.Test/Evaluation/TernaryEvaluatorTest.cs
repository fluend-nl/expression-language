using System.Collections.Generic;
using Fluend.ExpressionLanguage;
using FluentAssertions;
using NUnit.Framework;

namespace Fluend.ExpressionLanguage.Test.Evaluation
{
    public class TernaryEvaluatorTest
    {
        [Test]
        public void It_Can_Evaluate_A_Ternary_Operator()
        {
            var expression = "true ? true : false";
            var result = Expression.Run(expression);

            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be(true);
        }
        
        [Test]
        public void It_Support_Expression_Within_The_Result_Operands()
        {
            var expression = "4 == 4 ? 3 + 4 : false";
            var result = Expression.Run(expression);

            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be(7);
        }
        
        [Test]
        public void It_Gets_Correctly_Evaluated_As_An_Operand()
        {
            var expression = "3 + (true ? 4 : 5)";
            var result = Expression.Run(expression);

            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be(7);
        }
        
        [Test]
        public void It_Can_Evaluate_A_Ternary_Operator_In_An_Array_Expression()
        {
            var expression = "[1 > 2 ? 3 : 4]";
            var result = Expression.Run(expression);

            result.Succeeded.Should().BeTrue();
            ((List<object?>)result.Value!)[0].Should().Be(4);
        }
    }
}
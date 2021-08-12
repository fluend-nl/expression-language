using System;
using System.Collections.Generic;
using System.Linq;
using Fluend.ExpressionLanguage.Evaluation;
using Fluend.ExpressionLanguage.Evaluation.Functions;
using Fluend.ExpressionLanguage.Exceptions;
using Fluend.ExpressionLanguage.Lexing;
using Fluend.ExpressionLanguage.Parsing;
using NUnit.Framework;

namespace Fluend.ExpressionLanguage.Test.Evaluation
{
    public class TypeResolverTest
    {
        [Test]
        public void It_Resolves_A_Numeric_Type()
        {
            var expr = Eval("1");
            var resolver = new TypeResolver();

            var result = resolver.Resolve(expr, new Dictionary<string, object?>());
            Assert.AreEqual(result, typeof(double));
        }

        [Test]
        public void It_Resolves_A_String_Type()
        {
            var expr = Eval("'abc'");
            var resolver = new TypeResolver();

            var result = resolver.Resolve(expr, new Dictionary<string, object?>());
            Assert.AreEqual(result, typeof(string));
        }

        [Test]
        public void It_Resolves_A_Boolean_Type()
        {
            var expr = Eval("true");
            var resolver = new TypeResolver();

            var result = resolver.Resolve(expr, new Dictionary<string, object?>());
            Assert.AreEqual(result, typeof(bool));
        }

        [Test]
        public void It_Resolves_A_Binary_Expression()
        {
            var expr = Eval("3 == 3");
            var resolver = new TypeResolver();

            var result = resolver.Resolve(expr, new Dictionary<string, object?>());
            Assert.AreEqual(result, typeof(bool));
        }

        [Test]
        public void It_Resolves_A_Function_Expression()
        {
            var functions = new ExpressiveFunctionSet();
            var function = new ExpressiveFunction<Func<double>>("foo", () => 42);

            functions.Add(function);

            var expr = Eval("foo()", functions);
            var resolver = new TypeResolver(functions);

            var result = resolver.Resolve(expr, new Dictionary<string, object?>());
            Assert.AreEqual(result, typeof(double));
        }

        [Test]
        public void It_Throws_An_Exception_If_An_Overload_Could_Not_Be_Found()
        {
            var functions = new ExpressiveFunctionSet();
            var function = new ExpressiveFunction<Func<double, double>>(
                "foo", val => val * 2);

            functions.Add(function);

            var expr = Eval("foo()", functions);
            var resolver = new TypeResolver(functions);

            Assert.Throws<NoOverloadException>(() => { resolver.Resolve(expr, new Dictionary<string, object?>()); },
                "No suitable overload was found for 'foo()'");
        }

        [Test]
        public void It_Resolves_A_Nested_Function_Expression()
        {
            var functions = new ExpressiveFunctionSet();

            functions.Add(
                new ExpressiveFunction<Func<double, double>>("foo", val => val * 2));
            functions.Add(
                new ExpressiveFunction<Func<double>>("bar", () => 42));

            var expr = Eval("foo(bar())", functions);
            var resolver = new TypeResolver(functions);

            var result = resolver.Resolve(expr, new Dictionary<string, object?>());
            Assert.AreEqual(result, typeof(double));
        }

        [Test]
        public void It_Resolves_The_Correct_Overload_Function_Expression()
        {
            var functions = new ExpressiveFunctionSet();

            functions.Add(
                new ExpressiveFunction<Func<double, int>>("foo", val => (int) (val * 2)));
            functions.Add(
                new ExpressiveFunction<Func<double>>("foo", () => 42));

            var expr = Eval("foo(42)", functions);
            var resolver = new TypeResolver(functions);

            var result = resolver.Resolve(expr, new Dictionary<string, object?>());
            Assert.AreEqual(result, typeof(int));
        }

        [Test]
        public void It_Resolves_Functions_That_Take_HashTables()
        {
            var functions = new ExpressiveFunctionSet();
            var function = new ExpressiveFunction<Action<Dictionary<string, object>>>(
                "foo", table => Console.WriteLine(table["test"]));

            functions.Add(function);


            var expr = Eval("foo({ test: 123 })", functions);
            var resolver = new TypeResolver(functions);

            var result = resolver.Resolve(expr, new Dictionary<string, object?>());
            Assert.AreEqual(result, typeof(void));
        }

        [Test]
        public void It_Resolves_Return_Types_Of_Functions_That_Produce_HashTables()
        {
            var functions = new ExpressiveFunctionSet();
            var function = new ExpressiveFunction<Func<Dictionary<string, object>>>(
                "foo", () => new Dictionary<string, object>
                {
                    {"test", 123}
                });

            functions.Add(function);

            var expr = Eval("foo()", functions);
            var resolver = new TypeResolver(functions);

            var result = resolver.Resolve(expr, new Dictionary<string, object?>());
            Assert.AreEqual(result, typeof(Dictionary<string, object>));
        }

        [Test]
        public void It_Resolves_Functions_That_Take_Arrays()
        {
            var functions = new ExpressiveFunctionSet();
            var function = new ExpressiveFunction<Action<List<object>>>(
                "foo", array => Console.WriteLine(array[0]));

            functions.Add(function);

            var expr = Eval("foo([1, 2])", functions);
            var resolver = new TypeResolver(functions);

            var result = resolver.Resolve(expr, new Dictionary<string, object?>());
            Assert.AreEqual(result, typeof(void));
        }

        [Test]
        public void It_Resolves_Return_Types_Of_Functions_That_Produce_Arrays()
        {
            var functions = new ExpressiveFunctionSet();
            var function = new ExpressiveFunction<Func<List<object>>>(
                "foo", () => new List<object>
                {
                    123
                });

            functions.Add(function);

            var expr = Eval("foo()", functions);
            var resolver = new TypeResolver(functions);

            var result = resolver.Resolve(expr, new Dictionary<string, object?>());
            Assert.AreEqual(result, typeof(List<object>));
        }
        
        [Test]
        public void It_Resolves_The_Return_Type_Of_Concatenated_Strings()
        {
            var expr = Eval("'abc: ' ~ 123");
            var resolver = new TypeResolver();

            var result = resolver.Resolve(expr, new Dictionary<string, object?>());
            Assert.AreEqual(result, typeof(string));
        }
        
        [Test]
        public void It_Resolves_The_Return_Type_Of_Numeric_Ranges()
        {
            var expr = Eval("18..68");
            var resolver = new TypeResolver();

            var result = resolver.Resolve(expr, new Dictionary<string, object?>());
            Assert.AreEqual(result, typeof(List<object>));
        }

        private Node Eval(string expression, ExpressiveFunctionSet? functions = null)
        {
            return Eval(expression, new(), functions);
        }

        private Node Eval(string expression, List<string> variables, ExpressiveFunctionSet? functions = null)
        {
            var parser = new Parser(functions?.GetFunctionNames().ToList() ?? new List<string>());
            var lexer = new Lexer();

            return parser.Parse(lexer.Tokenize(expression), variables);
        }
    }
}
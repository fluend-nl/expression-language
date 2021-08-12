namespace Fluend.ExpressionLanguage
{
    public enum TokenType
    {
        Eof,
        Name,
        Number,
        String,
        Operator,
        Punctuation
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Cursor { get; }
        
        public Token(TokenType type, string value, int cursor)
        {
            Type = type;
            Value = value;
            Cursor = cursor;
        }

        public bool Test(TokenType type)
        {
            return type == Type;
        }

        public bool Test(TokenType type, string value)
        {
            return type == Type && value == Value;
        }
    }
}
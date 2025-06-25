using System;
using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class ArithmeticResolver
    {
        private readonly ArgumentParser _argumentParser;
        private int _index;
        private List<Token> _tokens;
        
        public ArithmeticResolver(ArgumentParser argumentParser)
        {
            _argumentParser = argumentParser;
        }

        public double Evaluate(List<string> tokens, FunctionContext context)
        {
            _tokens = new List<Token>();
            foreach (var token in tokens)
            {
                if (token == "(")
                {
                    _tokens.Add(new Token(TokenType.Parenthesis, "("));
                }
                else if (token == ")")
                {
                    _tokens.Add(new Token(TokenType.Parenthesis, ")"));
                }
                else if (token == "+" || token == "-" || token == "*" || token == "/" || token == "^")
                {
                    if(_tokens.Count > 0 && _tokens[^1].Type is TokenType.Operator)
                    {
                        throw new Exception($"Consecutive operators found: '{token}'");
                    }
                    
                    _tokens.Add(new Token(TokenType.Operator, token));
                }
                else if (token == "%")
                {
                    if(_tokens.Count > 0 && _tokens[^1].Type is TokenType.Operator or TokenType.Percentage)
                    {
                        throw new Exception($"Consecutive operators found: '{token}'");
                    }
                    
                    _tokens.Add(new Token(TokenType.Percentage, token));
                }
                else if(!string.IsNullOrEmpty(token.Trim())) {
                    if(!FunctionArgumentHelper.TryGetSingleNumber(_argumentParser.ParseArgument(token, context, new ArgumentDefinition(ArgumentType.SingleNumber, "")), out var num))
                    {
                        throw new Exception($"Unexpected token in arithmetic expression: '{token}'");
                    }
                    
                    _tokens.Add(new Token(TokenType.Number, num));
                }
            }

            _index = 0;
            double result = ParseExpression();
            if (_index != _tokens.Count)
            {
                throw new Exception("Unexpected tokens at the end of expression.");
            }
            return result;
        }

        private double ParseExpression()
        {
            double left = ParseTerm();
            while (_index < _tokens.Count)
            {
                Token token = _tokens[_index];
                if (token.Type != TokenType.Operator || (token.Value as string != "+" && token.Value as string != "-"))
                {
                    break;
                }
                _index++;
                double right = ParseTerm();
                if (token.Value as string == "+")
                {
                    left += right;
                }
                else
                {
                    left -= right;
                }
            }
            return left;
        }

        private double ParseTerm()
        {
            double left = ParseExponent();
            while (_index < _tokens.Count)
            {
                Token token = _tokens[_index];
                if (token.Type != TokenType.Operator || (token.Value as string != "*" && token.Value as string != "/"))
                {
                    break;
                }
                _index++;
                double right = ParseExponent();
                string op = token.Value as string;
                switch (op)
                {
                    case "*":
                        left *= right;
                        break;
                    case "/":
                        if (right == 0)
                        {
                            throw new DivideByZeroException();
                        }
                        left /= right;
                        break;
                }
            }
            return left;
        }

        private double ParseExponent()
        {
            double left = ParseFactor();
            while (_index < _tokens.Count)
            {
                Token token = _tokens[_index];
                if (token.Type != TokenType.Operator || (token.Value as string != "^"))
                {
                    break;
                }
                _index++;
                double right = ParseExponent(); // Right-associative: a^b^c = a^(b^c)
                left = Math.Pow(left, right);
            }
            return left;
        }

        private double ParseFactor()
        {
            if (_index >= _tokens.Count)
            {
                throw new Exception("Unexpected end of expression");
            }

            Token token = _tokens[_index];
            if (token.Type == TokenType.Operator && (token.Value as string == "+" || token.Value as string == "-"))
            {
                _index++;
                double factor = ParseFactor();
                return token.Value as string == "+" ? factor : -factor;
            }
            
            double result = ParsePrimary();
            
            // Handle postfix percentage operators
            while (_index < _tokens.Count && _tokens[_index].Type == TokenType.Percentage)
            {
                _index++;
                result /= 100.0;
            }
            
            return result;
        }

        private double ParsePrimary()
        {
            if (_index >= _tokens.Count)
            {
                throw new Exception("Unexpected end of expression");
            }

            Token token = _tokens[_index];
            _index++;
            if (token.Type == TokenType.Number)
            {
                return (double)token.Value;
            }
            if (token.Type == TokenType.Parenthesis && (string)token.Value == "(")
            {
                double expr = ParseExpression();
                if (_index >= _tokens.Count || _tokens[_index].Type != TokenType.Parenthesis || (string)_tokens[_index].Value != ")")
                {
                    throw new Exception("Expected ')'");
                }
                _index++;
                return expr;
            }
            throw new Exception($"Unexpected token: {token.Value}");
        }
            
        private enum TokenType
        {
            Number,
            Operator,
            Parenthesis,
            Percentage
        }

        private class Token
        {
            public TokenType Type { get; }
            public object Value { get; }

            public Token(TokenType type, object value)
            {
                Type = type;
                Value = value;
            }
        }
    }
}
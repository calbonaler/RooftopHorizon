using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RooftopHorizon.Models
{
	public sealed class Tokenizer : IEnumerator<Token>
	{
		public Tokenizer(TextReader reader) { this.reader = reader; }

		TextReader reader;
		List<Token> tokens = new List<Token>();
		string line = null;
		int index = 0;
		int state = 0;
		int offset = -1;

		public bool MoveNext()
		{
			if (reader == null)
				throw new ObjectDisposedException(GetType().ToString());
			if (++offset < tokens.Count)
				return true;
			switch (state)
			{
				case 0:
					state = -1;
					if ((line = reader.ReadLine()) != null)
					{
						index = 0;
						goto case 1;
					}
					return false;
				case 1:
					while (index < line.Length && char.IsWhiteSpace(line[index]))
						index++;
					if (index < line.Length)
					{
						Token token;
						if ((token = ParseString()) != null) { }
						else if ((token = ParseNumber()) != null) { }
						else if ((token = ParseKeywordOrString()) != null) { }
						else
						{
							var ind = index;
							StringBuilder sb = new StringBuilder();
							while (index < line.Length && !char.IsWhiteSpace(line[index]))
								sb.Append(line[index++]);
							TokenType type = TokenType.Unknown;
							while ((type = Token.GetTokenTypeForSymbol(sb.ToString())) == TokenType.Unknown)
								sb.Remove(sb.Length - 1, 1);
							token = new Token(null, type);
							index = ind + sb.Length;
						}
						tokens.Add(token);
						state = 1;
						return true;
					}
					goto case 0;
				default:
					return false;
			}
		}

		public bool MovePrevious()
		{
			if (reader == null)
				throw new ObjectDisposedException(GetType().ToString());
			return --offset >= 0;
		}

		public Token Current
		{
			get
			{
				if (reader == null)
					throw new ObjectDisposedException(GetType().ToString());
				return tokens[offset];
			}
		}

		public void Dispose()
		{
			if (reader != null)
			{
				reader.Dispose();
				reader = null;
			}
			if (tokens != null)
				tokens = null;
			if (line != null)
				line = null;
		}

		object System.Collections.IEnumerator.Current { get { return Current; } }

		void System.Collections.IEnumerator.Reset() { throw new NotSupportedException(); }

		Token ParseKeywordOrString()
		{
			if (!Token.Symbols.Keys.Any(s => s[0] == line[index]))
			{
				StringBuilder sb = new StringBuilder();
				while (index < line.Length && !char.IsWhiteSpace(line[index]) && !Token.Symbols.Keys.Any(s => s[0] == line[index]))
					sb.Append(line[index++]);
				return Token.CreateTokenFromKeywordOrString(sb.ToString());
			}
			return null;
		}

		Token ParseNumber()
		{
			var ind = index;
			if (char.IsDigit(line[index]) || line[index] == '+' || line[index] == '-')
			{
				bool relative = false;
				int pm = 1;
				if (line[index] == '+')
				{
					relative = true;
					index++;
				}
				else if (line[index] == '-')
				{
					relative = true;
					pm = -1;
					index++;
				}
				int integer = 0;
				while (index < line.Length)
				{
					if (char.IsDigit(line[index]))
						integer = integer * 10 + line[index++] - '0';
					else if (char.IsLetter(line[index]))
					{
						index = ind;
						return null;
					}
					else
						break;
				}
				integer *= pm;
				return new Token(integer, relative ? TokenType.RelativeInteger : TokenType.Integer);
			}
			return null;
		}

		Token ParseString()
		{
			if (line[index] == '"')
			{
				index++;
				StringBuilder sb = new StringBuilder();
				while (index < line.Length)
				{
					if (line[index] == '\\')
					{
						index++;
						try
						{
							var ch = line[index++];
							if (ch == 'a')
								sb.Append('\a');
							else if (ch == 'b')
								sb.Append('\b');
							else if (ch == 'f')
								sb.Append('\f');
							else if (ch == 'n')
								sb.Append('\n');
							else if (ch == 'r')
								sb.Append('\r');
							else if (ch == 't')
								sb.Append('\t');
							else if (ch == 'v')
								sb.Append('\v');
							else
								sb.Append(ch);
						}
						catch (IndexOutOfRangeException ex)
						{
							throw new ArgumentException("入力文字列が正しくありません。", ex);
						}
					}
					else
						sb.Append(line[index++]);
					if (line[index] == '"')
					{
						index++;
						break;
					}
				}
				return new Token(sb.ToString(), TokenType.String);
			}
			return null;
		}
	}
}

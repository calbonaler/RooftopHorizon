using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RooftopHorizon.Query
{
	public sealed class Tokenizer : IDisposable
	{
		public Tokenizer(TextReader reader)
		{
			m_Reader = reader;
			Read();
		}

		TextReader m_Reader;
		Token m_Next;
		string m_Line = null;
		int m_Index = 0;

		public Token Read()
		{
			var before = m_Next;
			if (m_Reader == null)
				throw new ObjectDisposedException(GetType().ToString());
			if ((m_Line == null || m_Index >= m_Line.Length) && (m_Line = m_Reader.ReadLine()) == null)
			{
				m_Next = new EndOfStreamToken();
				return before;
			}
			while (m_Index < m_Line.Length && char.IsWhiteSpace(m_Line[m_Index]))
				m_Index++;
			if (m_Index >= m_Line.Length)
			{
				m_Next = new EndOfStreamToken();
				return before;
			}
			Token token;
			if ((token = ParseString()) != null) { }
			else if ((token = ParseNumber()) != null) { }
			else if ((token = ParseKeywordOrString()) != null) { }
			else
			{
				int successfulIndex = -1;
				SymbolToken symbol = null;
				StringBuilder sb = new StringBuilder();
				while (m_Index < m_Line.Length && !char.IsWhiteSpace(m_Line[m_Index]))
				{
					sb.Append(m_Line[m_Index++]);
					SymbolToken res;
					if (Token.Symbols.TryGetValue(sb.ToString(), out res))
					{
						symbol = res;
						successfulIndex = m_Index;
					}
				}
				if (symbol != null)
				{
					m_Index = successfulIndex;
					token = symbol;
				}
				else
					token = new UnknownToken(sb.ToString());
			}
			m_Next = token;
			return before;
		}

		public Token Next
		{
			get
			{
				if (m_Reader == null)
					throw new ObjectDisposedException(GetType().ToString());
				return m_Next;
			}
		}

		public void Dispose()
		{
			if (m_Reader != null)
			{
				m_Reader.Dispose();
				m_Reader = null;
			}
			if (m_Next != null)
				m_Next = null;
			if (m_Line != null)
				m_Line = null;
		}

		Token ParseKeywordOrString()
		{
			if (char.IsLetterOrDigit(m_Line[m_Index]) || m_Line[m_Index] == '_')
			{
				StringBuilder sb = new StringBuilder();
				while (m_Index < m_Line.Length && (char.IsLetterOrDigit(m_Line[m_Index]) || m_Line[m_Index] == '_'))
					sb.Append(m_Line[m_Index++]);
				KeywordToken keyword = null;
				if (Token.Keywords.TryGetValue(sb.ToString(), out keyword))
					return keyword;
				return new StringToken(sb.ToString());
			}
			return null;
		}

		Token ParseNumber()
		{
			var start = m_Index;
			if (char.IsDigit(m_Line[m_Index]) || m_Line[m_Index] == '+' || m_Line[m_Index] == '-')
			{
				bool relative = false;
				bool negative = false;
				if (m_Line[m_Index] == '+')
				{
					relative = true;
					m_Index++;
				}
				else if (m_Line[m_Index] == '-')
				{
					relative = negative = true;
					m_Index++;
				}
				int integer = 0;
				while (m_Index < m_Line.Length)
				{
					if (char.IsDigit(m_Line[m_Index]))
						integer = integer * 10 + m_Line[m_Index++] - '0';
					else if (char.IsLetter(m_Line[m_Index]))
					{
						m_Index = start;
						return null;
					}
					else
						break;
				}
				if (relative)
					return new RelativeIntegerToken(negative ? -integer : integer);
				else
					return new AbsoluteIntegerToken(negative ? -integer : integer);
			}
			return null;
		}

		Token ParseString()
		{
			if (m_Line[m_Index] == '"')
			{
				m_Index++;
				StringBuilder sb = new StringBuilder();
				while (m_Index < m_Line.Length)
				{
					if (m_Line[m_Index] == '\\')
					{
						m_Index++;
						try
						{
							var ch = m_Line[m_Index++];
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
						sb.Append(m_Line[m_Index++]);
					if (m_Line[m_Index] == '"')
					{
						m_Index++;
						break;
					}
				}
				return new StringToken(sb.ToString());
			}
			return null;
		}
	}
}

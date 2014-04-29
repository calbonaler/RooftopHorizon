using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RooftopHorizon.Query
{
	public abstract class Token
	{
		public static readonly IReadOnlyDictionary<string, SymbolToken> Symbols = MakeTable<SymbolToken>();
		public static readonly IReadOnlyDictionary<string, KeywordToken> Keywords = MakeTable<KeywordToken>();

		static IReadOnlyDictionary<string, T> MakeTable<T>() where T : Token
		{
			Dictionary<string, T> types = new Dictionary<string, T>();
			foreach (var type in typeof(T).Assembly.GetTypes())
			{
				if (type.IsSubclassOf(typeof(T)))
				{
					var token = (T)Activator.CreateInstance(type);
					types.Add(token.StringifiedValue, token);
				}
			}
			return types;
		}

		public abstract string StringifiedValue { get; }
	}

	public sealed class EndOfStreamToken : Token
	{
		public override string StringifiedValue { get { return "end of stream"; } }
	}

	public sealed class UnknownToken : Token
	{
		public UnknownToken(string value) { m_Value = value; }

		string m_Value;

		public override string StringifiedValue { get { return m_Value; } }
	}

	public sealed class StringToken : Token
	{
		public StringToken(string value) { Value = value; }

		public string Value { get; private set; }

		public override string StringifiedValue { get { return Value; } }
	}

	public sealed class AbsoluteIntegerToken : Token
	{
		public AbsoluteIntegerToken(int value) { Value = value; }

		public int Value { get; private set; }

		public override string StringifiedValue { get { return Value.ToString(); } }
	}

	public sealed class RelativeIntegerToken : Token
	{
		public RelativeIntegerToken(int value) { Value = value; }

		public int Value { get; private set; }

		public override string StringifiedValue { get { return Value.ToString("+0;-0;0"); } }
	}

	public abstract class KeywordToken : Token { }

	public sealed class SelectToken : KeywordToken
	{
		public override string StringifiedValue { get { return "sel"; } }
	}

	public sealed class FavoriteToken : KeywordToken
	{
		public override string StringifiedValue { get { return "fav"; } }
	}

	public sealed class UnfavoriteToken : KeywordToken
	{
		public override string StringifiedValue { get { return "unfav"; } }
	}

	public sealed class RetweetToken : KeywordToken
	{
		public override string StringifiedValue { get { return "rt"; } }
	}

	public sealed class ReplyToken : KeywordToken
	{
		public override string StringifiedValue { get { return "re"; } }
	}

	public sealed class CountToken : KeywordToken
	{
		public override string StringifiedValue { get { return "count"; } }
	}

	public sealed class WhereToken : KeywordToken
	{
		public override string StringifiedValue { get { return "where"; } }
	}

	public sealed class OnToken : KeywordToken
	{
		public override string StringifiedValue { get { return "on"; } }
	}

	public sealed class AndToken : KeywordToken
	{
		public override string StringifiedValue { get { return "and"; } }
	}

	public sealed class OrToken : KeywordToken
	{
		public override string StringifiedValue { get { return "or"; } }
	}

	public sealed class NotToken : KeywordToken
	{
		public override string StringifiedValue { get { return "not"; } }
	}

	public sealed class ByToken : KeywordToken
	{
		public override string StringifiedValue { get { return "by"; } }
	}

	public sealed class ViaToken : KeywordToken
	{
		public override string StringifiedValue { get { return "via"; } }
	}

	public sealed class ContainsToken : KeywordToken
	{
		public override string StringifiedValue { get { return "contains"; } }
	}

	public sealed class MatchesToken : KeywordToken
	{
		public override string StringifiedValue { get { return "matches"; } }
	}

	public sealed class HomeToken : KeywordToken
	{
		public override string StringifiedValue { get { return "home"; } }
	}

	public sealed class MentionsToken : KeywordToken
	{
		public override string StringifiedValue { get { return "mentions"; } }
	}

	public sealed class UserToken : KeywordToken
	{
		public override string StringifiedValue { get { return "user"; } }
	}

	public abstract class SymbolToken : Token { }

	public sealed class LeftParenthesisToken : SymbolToken
	{
		public override string StringifiedValue { get { return "("; } }
	}

	public sealed class RightParenthesisToken : SymbolToken
	{
		public override string StringifiedValue { get { return ")"; } }
	}
}

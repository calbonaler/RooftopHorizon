using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Saruna;

namespace RooftopHorizon.Query
{
	public sealed class Parser : IDisposable
	{
		public Parser(TextReader reader, Model model)
		{
			m_Tokenizer = new Tokenizer(reader);
			m_Model = model;
		}

		public void Dispose()
		{
			if (m_Tokenizer != null)
			{
				m_Tokenizer.Dispose();
				m_Tokenizer = null;
			}
		}

		Tokenizer m_Tokenizer;
		Model m_Model;

		T Expect<T>() where T : Token
		{
			if (m_Tokenizer.Next is T)
				return (T)m_Tokenizer.Read();
			string expectedTokenString = null;
			if (typeof(T).IsSubclassOf(typeof(SymbolToken)))
			{
				var expectedToken = Token.Symbols.Values.FirstOrDefault(x => x is T);
				if (expectedToken != null)
					expectedTokenString = expectedToken.StringifiedValue;
			}
			else if (typeof(T).IsSubclassOf(typeof(KeywordToken)))
			{
				var expectedToken = Token.Keywords.Values.FirstOrDefault(x => x is T);
				if (expectedToken != null)
					expectedTokenString = expectedToken.StringifiedValue;
			}
			if (expectedTokenString == null)
				expectedTokenString = typeof(T).Name.Replace("Token", "");
			throw new ArgumentException(string.Format("トークン {0} が予期されましたがトークン {1} に遭遇しました。", expectedTokenString, m_Tokenizer.Next.StringifiedValue));
		}

		bool Accept<T>() where T : Token
		{
			if (m_Tokenizer.Next is T)
			{
				m_Tokenizer.Read();
				return true;
			}
			return false;
		}

		public Command Parse()
		{
			List<Func<Tweet, Task>> commands = null;
			var command = ParseCommandName();
			if (command != null)
				(commands ?? (commands = new List<Func<Tweet, Task>>())).Add(command);
			while (Accept<AndToken>())
			{
				command = ParseCommandName();
				if (command == null || commands == null)
					throw new ArgumentException("select 操作は他の操作と組み合わせることはできません。");
				commands.Add(command);
			}
			var parameter = new CommandParameter();
			parameter.Predicate = t => true;
			parameter.Count = 1;
			while (true)
			{
				if (m_Tokenizer.Next is AbsoluteIntegerToken)
					parameter.Position = ((AbsoluteIntegerToken)m_Tokenizer.Read()).Value;
				else if (m_Tokenizer.Next is RelativeIntegerToken)
				{
					parameter.Position = ((RelativeIntegerToken)m_Tokenizer.Read()).Value;
					parameter.Relative = true;
				}
				else if (Accept<AllToken>())
					parameter.Count = null;
				else if (Accept<CountToken>())
					parameter.Count = Expect<AbsoluteIntegerToken>().Value;
				else if (Accept<OnToken>())
				{
					if (Accept<HomeToken>())
						parameter.Timeline = TimelineType.Home;
					else if (Accept<MentionsToken>())
						parameter.Timeline = TimelineType.Mention;
					else
					{
						Expect<UserToken>();
						parameter.Timeline = TimelineType.User;
						parameter.OwnerScreenName = Expect<StringToken>().Value;
					}
				}
				else if (Accept<WhereToken>())
				{
					var tweet = Expression.Parameter(typeof(Tweet));
					parameter.Predicate = Expression.Lambda<Func<Tweet, bool>>(ParseExpression(tweet), tweet).Compile();
				}
				else
					break;
			}
			if (commands != null)
			{
				foreach (var com in commands)
					parameter.Executors.Add(com);
			}
			return new Command(parameter, m_Model);
		}

		Func<Tweet, Task> ParseCommandName()
		{
			if (Accept<FavoriteToken>())
				return m_Model.Twitter.FavoriteAsync;
			if (Accept<UnfavoriteToken>())
				return m_Model.Twitter.UnfavoriteAsync;
			if (Accept<RetweetToken>())
				return m_Model.Twitter.RetweetAsync;
			if (Accept<ReplyToken>())
				return async x => { m_Model.SetInReplyTo(x); await Task.Delay(0); };
			Expect<SelectToken>();
			return null;
		}

		Expression ParseExpression(ParameterExpression tweet)
		{
			var left = ParseAndExpression(tweet);
			while (Accept<OrToken>())
				left = Expression.Or(left, ParseAndExpression(tweet));
			return left;
		}

		Expression ParseAndExpression(ParameterExpression tweet)
		{
			var left = ParseNotExpression(tweet);
			while (Accept<AndToken>())
				left = Expression.And(left, ParseNotExpression(tweet));
			return left;
		}

		Expression ParseNotExpression(ParameterExpression tweet)
		{
			if (Accept<NotToken>())
				return Expression.Not(ParsePrimitiveExpression(tweet));
			return ParsePrimitiveExpression(tweet);
		}

		Expression ParsePrimitiveExpression(ParameterExpression tweet)
		{
			if (Accept<LeftParenthesisToken>())
			{
				var exp = ParseExpression(tweet);
				Expect<RightParenthesisToken>();
				return exp;
			}
			var stringIndexOf = typeof(string).GetMethod("IndexOf", new[] { typeof(string), typeof(StringComparison) });
			if (Accept<ByToken>())
				return Expression.GreaterThanOrEqual(
					Expression.Call(
						Expression.Property(Expression.Property(tweet, "User"), "ScreenName"),
						stringIndexOf,
						Expression.Constant(Expect<StringToken>().Value),
						Expression.Constant(StringComparison.OrdinalIgnoreCase)
					),
					Expression.Constant(0)
				);
			if (Accept<ViaToken>())
				return Expression.GreaterThanOrEqual(
					Expression.Call(
						Expression.Property(Expression.Property(tweet, "Source"), "InnerText"),
						stringIndexOf,
						Expression.Constant(Expect<StringToken>().Value),
						Expression.Constant(StringComparison.OrdinalIgnoreCase)
					),
					Expression.Constant(0)
				);
			if (Accept<ContainsToken>())
				return Expression.GreaterThanOrEqual(
					Expression.Call(
						Expression.Property(tweet, "Text"),
						stringIndexOf,
						Expression.Constant(Expect<StringToken>().Value),
						Expression.Constant(StringComparison.OrdinalIgnoreCase)
					),
					Expression.Constant(0)
				);
			var match = typeof(Regex).GetMethod("IsMatch", new[] { typeof(string), typeof(string), typeof(RegexOptions) });
			Expect<MatchesToken>();
			return Expression.Call(
				match,
				Expression.Property(tweet, "Text"),
				Expression.Constant(Expect<StringToken>().Value),
				Expression.Constant(RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture)
			);
		}
	}
}

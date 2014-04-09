using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Saruna;

namespace RooftopHorizon.Models
{
	public sealed class Parser : IDisposable
	{
		public Parser(TextReader reader, Model model)
		{
			tokenizer = new Tokenizer(reader);
			m_Model = model;
		}

		Tokenizer tokenizer;
		Model m_Model;

		public void Dispose()
		{
			if (tokenizer != null)
			{
				tokenizer.Dispose();
				tokenizer = null;
			}
			if (m_Model != null)
				m_Model = null;
		}

		public IEnumerable<Command> Parse()
		{
			List<Command> commands = new List<Command>();
			while (true)
			{
				if (!tokenizer.MoveNext())
					break;
				tokenizer.MovePrevious();
				commands.Add(ParseCommand());
				if (!tokenizer.MoveNext())
					break;
				if (tokenizer.Current.Type != TokenType.SymbolSemicolon)
					throw new ArgumentException();
			}
			return commands;
		}

		Command ParseCommand()
		{
			tokenizer.MoveNext();
			switch (tokenizer.Current.Type)
			{
				case TokenType.KeywordSelect:
					return new SelectCommand(ParseCommandParameter(), m_Model);
				case TokenType.KeywordFavorite:
					return new TweetRelatedCommand(ParseCommandParameter(), m_Model, m_Model.Twitter.FavoriteAsync);
				case TokenType.KeywordUnfavorite:
					return new TweetRelatedCommand(ParseCommandParameter(), m_Model, m_Model.Twitter.UnfavoriteAsync);
				case TokenType.KeywordRetweet:
					return new TweetRelatedCommand(ParseCommandParameter(), m_Model, m_Model.Twitter.RetweetAsync);
				case TokenType.KeywordReply:
					return new TweetRelatedCommand(ParseCommandParameter(), m_Model, async t => { m_Model.SetInReplyTo(t); await Task.Delay(0); });
				default:
					throw new ArgumentException();
			}
		}

		CommandParameter ParseCommandParameter()
		{
			CommandParameter parameter = new CommandParameter();
			if (!tokenizer.MoveNext() ||
				tokenizer.Current.Type != TokenType.Integer &&
				tokenizer.Current.Type != TokenType.RelativeInteger)
				tokenizer.MovePrevious();
			else
			{
				if (tokenizer.Current.Type == TokenType.RelativeInteger)
					parameter.Relative = true;
				parameter.Position = (int)tokenizer.Current.Value;
			}
			parameter.Count = 1;
			parameter.Predicate = t => true;
			while (true)
			{
				if (!tokenizer.MoveNext())
				{
					tokenizer.MovePrevious();
					break;
				}
				if (tokenizer.Current.Type == TokenType.KeywordCount)
				{
					if (!tokenizer.MoveNext() || tokenizer.Current.Type != TokenType.Integer)
						throw new ArgumentException();
					parameter.Count = (int)tokenizer.Current.Value;
				}
				else if (tokenizer.Current.Type == TokenType.KeywordWhere)
				{
					var tweet = Expression.Parameter(typeof(Tweet));
					parameter.Predicate = Expression.Lambda<Func<Tweet, bool>>(ParseOrExpression(tweet), tweet).Compile();
				}
				else if (tokenizer.Current.Type == TokenType.KeywordOn)
				{
					if (!tokenizer.MoveNext())
						throw new ArgumentException();
					else if (tokenizer.Current.Type == TokenType.KeywordHome)
						parameter.Timeline = TimelineType.Home;
					else if (tokenizer.Current.Type == TokenType.KeywordMentions)
						parameter.Timeline = TimelineType.Mention;
					else if (tokenizer.Current.Type == TokenType.KeywordUser)
					{
						if (!tokenizer.MoveNext() || tokenizer.Current.Type != TokenType.String)
							throw new ArgumentException();
						parameter.Timeline = TimelineType.User;
						parameter.OwnerScreenName = (string)tokenizer.Current.Value;
					}
					else
						throw new ArgumentException();
				}
				else
				{
					tokenizer.MovePrevious();
					break;
				}
			}
			if (tokenizer.MoveNext() && tokenizer.Current.Type == TokenType.SymbolLeftBrace)
			{
				while (true)
				{
					if (!tokenizer.MoveNext())
						throw new ArgumentException();
					if (tokenizer.Current.Type == TokenType.SymbolRightBrace)
						break;
					tokenizer.MovePrevious();
					parameter.SubCommands.Add(ParseCommand());
					if (!tokenizer.MoveNext())
						throw new ArgumentException();
					if (tokenizer.Current.Type != TokenType.SymbolSemicolon)
						break;
				}
				if (tokenizer.Current.Type != TokenType.SymbolRightBrace)
					throw new ArgumentException();
			}
			else
				tokenizer.MovePrevious();
			return parameter;
		}

		Expression ParseOrExpression(ParameterExpression tweet)
		{
			Expression exp = ParseAndExpression(tweet);
			while (true)
			{
				if (!tokenizer.MoveNext() || tokenizer.Current.Type != TokenType.KeywordOr)
				{
					tokenizer.MovePrevious();
					break;
				}
				exp = Expression.OrElse(exp, ParseAndExpression(tweet));
			}
			return exp;
		}

		Expression ParseAndExpression(ParameterExpression tweet)
		{
			Expression exp = ParseNotExpression(tweet);
			while (true)
			{
				if (!tokenizer.MoveNext() || tokenizer.Current.Type != TokenType.KeywordAnd)
				{
					tokenizer.MovePrevious();
					break;
				}
				exp = Expression.AndAlso(exp, ParseNotExpression(tweet));
			}
			return exp;
		}

		Expression ParseNotExpression(ParameterExpression tweet)
		{
			if (!tokenizer.MoveNext())
				throw new ArgumentException();
			bool not = false;
			if (tokenizer.Current.Type == TokenType.KeywordNot)
				not = true;
			else
				tokenizer.MovePrevious();
			var exp = ParseCondition(tweet);
			return not ? Expression.Not(exp) : exp;
		}

		Expression ParseCondition(ParameterExpression tweet)
		{
			if (!tokenizer.MoveNext())
				throw new ArgumentException();
			var stringIndexOf = typeof(string).GetMethod("IndexOf", new[] { typeof(string), typeof(StringComparison) });
			if (tokenizer.Current.Type == TokenType.KeywordBy)
			{
				if (!tokenizer.MoveNext() || tokenizer.Current.Type != TokenType.String)
					throw new ArgumentException();
				return Expression.GreaterThanOrEqual(
					Expression.Call(
						Expression.Property(Expression.Property(tweet, "User"), "ScreenName"),
						stringIndexOf,
						Expression.Constant(tokenizer.Current.Value),
						Expression.Constant(StringComparison.OrdinalIgnoreCase)
					),
					Expression.Constant(0)
				);
			}
			else if (tokenizer.Current.Type == TokenType.KeywordVia)
			{
				if (!tokenizer.MoveNext() || tokenizer.Current.Type != TokenType.String)
					throw new ArgumentException();
				return Expression.GreaterThanOrEqual(
					Expression.Call(
						Expression.Property(Expression.Property(tweet, "Source"), "InnerText"),
						stringIndexOf,
						Expression.Constant(tokenizer.Current.Value),
						Expression.Constant(StringComparison.OrdinalIgnoreCase)
					),
					Expression.Constant(0)
				);
			}
			else if (tokenizer.Current.Type == TokenType.KeywordContains)
			{
				if (!tokenizer.MoveNext() || tokenizer.Current.Type != TokenType.String)
					throw new ArgumentException();
				return Expression.GreaterThanOrEqual(
					Expression.Call(
						Expression.Property(tweet, "Text"),
						stringIndexOf,
						Expression.Constant(tokenizer.Current.Value),
						Expression.Constant(StringComparison.OrdinalIgnoreCase)
					),
					Expression.Constant(0)
				);
			}
			else if (tokenizer.Current.Type == TokenType.KeywordMatches)
			{
				if (!tokenizer.MoveNext() || tokenizer.Current.Type != TokenType.String)
					throw new ArgumentException();
				var match = typeof(Regex).GetMethod("IsMatch", new[] { typeof(string), typeof(string), typeof(RegexOptions) });
				return Expression.Call(
					match,
					Expression.Property(tweet, "Text"),
					Expression.Constant(tokenizer.Current.Value),
					Expression.Constant(RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture)
				);
			}
			else if (tokenizer.Current.Type == TokenType.SymbolLeftParenthesis)
			{
				var exp = ParseOrExpression(tweet);
				if (!tokenizer.MoveNext() || tokenizer.Current.Type != TokenType.SymbolRightParenthesis)
					throw new ArgumentException();
				return exp;
			}
			else
				throw new ArgumentException();
		}
	}
}

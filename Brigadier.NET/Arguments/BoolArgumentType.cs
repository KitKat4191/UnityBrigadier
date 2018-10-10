﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Brigadier.NET.Context;
using Brigadier.NET.Suggestion;

namespace Brigadier.NET.Arguments
{
	public class BoolArgumentType : ArgumentType<bool>
	{
		private static readonly IEnumerable<string> BoolExamples = new[] {"true", "false"};

		private BoolArgumentType()
		{
		}

		public static BoolArgumentType Bool()
		{
			return new BoolArgumentType();
		}

		public static bool GetBool<TSource>(CommandContext<TSource> context, string name)
		{
			return context.GetArgument<bool>(name);
		}

		public override bool Parse(IStringReader reader)
		{
			return reader.ReadBoolean();
		}

		public override Task<Suggestions> ListSuggestions<TSource>(CommandContext<TSource> context, SuggestionsBuilder builder)
		{
			if ("true".StartsWith(builder.Remaining.ToLower()))
			{
				builder.Suggest("true");
			}
			if ("false".StartsWith(builder.Remaining.ToLower()))
			{
				builder.Suggest("false");
			}
			return builder.BuildFuture();
		}

		public override IEnumerable<string> Examples => BoolExamples;
	}
}

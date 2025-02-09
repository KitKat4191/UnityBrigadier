// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using Brigadier.NET.Suggestion;
using Brigadier.NET.Tree;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Brigadier.NET.Tests.tree
{
	public class RootCommandNodeTest : AbstractCommandNodeTest {
		protected override CommandNode<object> GetCommandNode() {
			return GetNode;
		}

		private RootCommandNode<object> GetNode => new RootCommandNode<object>();

		[Test]
		public void TestParse(){
			RootCommandNode<object> _node = GetNode;
			
			var reader = new StringReader("hello world");
			_node.Parse(reader, new CommandContextBuilder<object>(new CommandDispatcher<object>(), new object(), new RootCommandNode<object>(), 0));
			reader.Cursor.Should().Be(0);
		}

		[Test]
		public void TestAddChildNoRoot(){
			RootCommandNode<object> _node = GetNode;
			
			_node.Invoking(n => n.AddChild(new RootCommandNode<object>()))
				.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void TestUsage(){
			RootCommandNode<object> _node = GetNode;
			
			_node.UsageText.Should().Be("");
		}

		[Test]
		public async Task TestSuggestions(){
			RootCommandNode<object> _node = GetNode;
			
			var context = Substitute.For<CommandContext<object>>(null, null, null, null, null, null, null, null, null, false);
			var result = await _node.ListSuggestions(context, new SuggestionsBuilder("", 0));
			result.IsEmpty().Should().Be(true);
		}

		[Test]// (expected = IllegalStateException.class)
		public void TestCreateBuilder(){
			RootCommandNode<object> _node = GetNode;
			
			_node.Invoking(n => n.CreateBuilder())
				.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void TestEquals(){
		
			new EqualsTester()
				.AddEqualityGroup(
					new RootCommandNode<object>(),
					new RootCommandNode<object>()
				)
				.AddEqualityGroup(
					new RootCommandNode<object> {
						LiteralArgumentBuilder<object>.LiteralArgument("foo").Build(),
					},
					new RootCommandNode<object> {
						LiteralArgumentBuilder<object>.LiteralArgument("foo").Build()
					}
				)
				.TestEquals();
		}
	}
}
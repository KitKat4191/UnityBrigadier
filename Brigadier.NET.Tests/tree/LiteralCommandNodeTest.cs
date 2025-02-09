// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;
using Brigadier.NET.Suggestion;
using Brigadier.NET.Tree;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Brigadier.NET.Tests.tree
{
    public class LiteralCommandNodeTest : AbstractCommandNodeTest
    {
        protected override CommandNode<object> GetCommandNode()
        {
            return GetNode;
        }

        private LiteralCommandNode<object> GetNode => LiteralArgumentBuilder<object>.LiteralArgument("foo").Build();

        private CommandContextBuilder<object> GetBuilder =>
            new CommandContextBuilder<object>(new CommandDispatcher<object>(), new object(),
                new RootCommandNode<object>(), 0);

        [Test]
        public void TestParse()
        {
            LiteralCommandNode<object> _node = GetNode;
            CommandContextBuilder<object> _contextBuilder = GetBuilder;

            var reader = new StringReader("foo bar");
            _node.Parse(reader, _contextBuilder);
            reader.Remaining.Should().BeEquivalentTo(" bar");
        }

        [Test]
        public void TestParseExact()
        {
            LiteralCommandNode<object> _node = GetNode;
            CommandContextBuilder<object> _contextBuilder = GetBuilder;

            var reader = new StringReader("foo");
            _node.Parse(reader, _contextBuilder);
            reader.Remaining.Should().BeEquivalentTo("");
        }

        [Test]
        public void TestParseSimilar()
        {
            LiteralCommandNode<object> _node = GetNode;
            CommandContextBuilder<object> _contextBuilder = GetBuilder;

            var reader = new StringReader("foobar");
            _node.Invoking(n => n.Parse(reader, _contextBuilder))
                .Should().Throw<CommandSyntaxException>()
                .Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.LiteralIncorrect())
                .Where(ex => ex.Cursor == 0);
        }

        [Test]
        public void TestParseInvalid()
        {
            LiteralCommandNode<object> _node = GetNode;
            CommandContextBuilder<object> _contextBuilder = GetBuilder;

            var reader = new StringReader("bar");
            _node.Invoking(n => n.Parse(reader, _contextBuilder))
                .Should().Throw<CommandSyntaxException>()
                .Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.LiteralIncorrect())
                .Where(ex => ex.Cursor == 0);
        }

        [Test]
        public void TestUsage()
        {
            LiteralCommandNode<object> _node = GetNode;

            _node.UsageText.Should().Be("foo");
        }

        [Test]
        public async Task TestSuggestions()
        {
            LiteralCommandNode<object> _node = GetNode;
            CommandContextBuilder<object> _contextBuilder = GetBuilder;

            var empty = await _node.ListSuggestions(_contextBuilder.Build(""), new SuggestionsBuilder("", 0));
            empty.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
                { new Suggestion.Suggestion(StringRange.At(0), "foo") });

            var foo = await _node.ListSuggestions(_contextBuilder.Build("foo"), new SuggestionsBuilder("foo", 0));
            foo.IsEmpty().Should().Be(true);

            var food = await _node.ListSuggestions(_contextBuilder.Build("food"), new SuggestionsBuilder("food", 0));
            food.IsEmpty().Should().Be(true);

            var b = await _node.ListSuggestions(_contextBuilder.Build("b"), new SuggestionsBuilder("b", 0));
            b.IsEmpty().Should().Be(true);
        }

        [Test]
        public void TestEquals()
        {
            var command = Substitute.For<Command<object>>();

            new EqualsTester()
                .AddEqualityGroup(
                    LiteralArgumentBuilder<object>.LiteralArgument("foo").Build(),
                    LiteralArgumentBuilder<object>.LiteralArgument("foo").Build()
                )
                .AddEqualityGroup(
                    LiteralArgumentBuilder<object>.LiteralArgument("bar").Executes(command).Build(),
                    LiteralArgumentBuilder<object>.LiteralArgument("bar").Executes(command).Build()
                )
                .AddEqualityGroup(
                    LiteralArgumentBuilder<object>.LiteralArgument("bar").Build(),
                    LiteralArgumentBuilder<object>.LiteralArgument("bar").Build()
                )
                .AddEqualityGroup(
                    LiteralArgumentBuilder<object>.LiteralArgument("foo")
                        .Then(LiteralArgumentBuilder<object>.LiteralArgument("bar")).Build(),
                    LiteralArgumentBuilder<object>.LiteralArgument("foo")
                        .Then(LiteralArgumentBuilder<object>.LiteralArgument("bar")).Build()
                )
                .TestEquals();
        }

        [Test]
        public void TestCreateBuilder()
        {
            LiteralCommandNode<object> _node = GetNode;

            var builder = (LiteralArgumentBuilder<object>)_node.CreateBuilder();
            builder.Literal.Should().Be(_node.Literal);
            builder.Requirement.Should().Be(_node.Requirement);
            builder.Command.Should().Be(_node.Command);
        }
    }
}
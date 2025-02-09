// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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
    public class ArgumentCommandNodeTest : AbstractCommandNodeTest
    {
        protected override CommandNode<object> GetCommandNode()
        {
            return GetNode;
        }

        private ArgumentCommandNode<object, int> GetNode => RequiredArgumentBuilder<object, int>
            .RequiredArgument("foo", Arguments.Integer()).Build();

        private CommandContextBuilder<object> GetBuilder =>
            new CommandContextBuilder<object>(new CommandDispatcher<object>(), new object(),
                new RootCommandNode<object>(), 0);

        [Test]
        public void TestParse()
        {
            ArgumentCommandNode<object, int> _node = GetNode;
            CommandContextBuilder<object> _contextBuilder = GetBuilder;

            var reader = new StringReader("123 456");
            _node.Parse(reader, _contextBuilder);

            _contextBuilder.GetArguments().ContainsKey("foo").Should().Be(true);
            _contextBuilder.GetArguments()["foo"].Result.Should().Be(123);
        }

        [Test]
        public void TestUsage()
        {
            ArgumentCommandNode<object, int> _node = GetNode;

            _node.UsageText.Should().Be("<foo>");
        }

        [Test]
        public async Task TestSuggestions()
        {
            ArgumentCommandNode<object, int> _node = GetNode;
            CommandContextBuilder<object> _contextBuilder = GetBuilder;

            var result = await _node.ListSuggestions(_contextBuilder.Build(""), new SuggestionsBuilder("", 0));
            result.IsEmpty().Should().Be(true);
        }

        [Test]
        public void TestEquals()
        {
            var command = Substitute.For<Command<object>>();

            new EqualsTester()
                .AddEqualityGroup(
                    RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer()).Build(),
                    RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer()).Build()
                )
                .AddEqualityGroup(
                    RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer()).Executes(command)
                        .Build(),
                    RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer()).Executes(command)
                        .Build()
                )
                .AddEqualityGroup(
                    RequiredArgumentBuilder<object, int>.RequiredArgument("bar", Arguments.Integer(-100, 100)).Build(),
                    RequiredArgumentBuilder<object, int>.RequiredArgument("bar", Arguments.Integer(-100, 100)).Build()
                )
                .AddEqualityGroup(
                    RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer(-100, 100)).Build(),
                    RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer(-100, 100)).Build()
                )
                .AddEqualityGroup(
                    RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer()).Then(
                        RequiredArgumentBuilder<object, int>.RequiredArgument("bar", Arguments.Integer())
                    ).Build(),
                    RequiredArgumentBuilder<object, int>.RequiredArgument("foo", Arguments.Integer()).Then(
                        RequiredArgumentBuilder<object, int>.RequiredArgument("bar", Arguments.Integer())
                    ).Build()
                )
                .TestEquals();
        }

        [Test]
        public void TestCreateBuilder()
        {
            ArgumentCommandNode<object, int> _node = GetNode;

            var builder = (RequiredArgumentBuilder<object, int>)_node.CreateBuilder();
            builder.Name.Should().Be(_node.Name);
            builder.Type.Should().Be(_node.Type);
            builder.Requirement.Should().Be(_node.Requirement);
            builder.Command.Should().Be(_node.Command);
        }
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.ArgumentTypes;
using Brigadier.NET.Builder;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Brigadier.NET.Tests.builder
{
    public class RequiredArgumentBuilderTest
    {
        private readonly ArgumentType<int> _type = Substitute.For<ArgumentType<int>>();

        [Test]
        public void TestBuild()
        {
            RequiredArgumentBuilder<object, int> builder =
                RequiredArgumentBuilder<object, int>.RequiredArgument("foo", _type);

            var node = builder.Build();

            node.Name.Should().Be("foo");
            node.Type.Should().Be(_type);
        }

        [Test]
        public void TestBuildWithExecutor()
        {
            RequiredArgumentBuilder<object, int> builder =
                RequiredArgumentBuilder<object, int>.RequiredArgument("foo", _type);
            Command<object> command = Substitute.For<Command<object>>();

            var node = builder.Executes(command).Build();

            node.Name.Should().Be("foo");
            node.Type.Should().Be(_type);
            node.Command.Should().Be(command);
        }

        [Test]
        public void TestBuildWithChildren()
        {
            RequiredArgumentBuilder<object, int> builder =
                RequiredArgumentBuilder<object, int>.RequiredArgument("foo", _type);

            builder.Then(r => r.Argument("bar", Arguments.Integer()));
            builder.Then(r => r.Argument("baz", Arguments.Integer()));
            var node = builder.Build();

            node.Children.Should().HaveCount(2);
        }
    }
}
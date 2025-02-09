// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.Builder;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Brigadier.NET.Tests.builder
{
    public class LiteralArgumentBuilderTest
    {
        [Test]
        public void TestBuild()
        {
            LiteralArgumentBuilder<object> builder = new LiteralArgumentBuilder<object>("foo");

            var node = builder.Build();

            node.Literal.Should().Be("foo");
        }

        [Test]
        public void TestBuildWithExecutor()
        {
            LiteralArgumentBuilder<object> builder = new LiteralArgumentBuilder<object>("foo");
            Command<object> command = Substitute.For<Command<object>>();

            var node = builder.Executes(command).Build();

            node.Literal.Should().Be("foo");
            node.Command.Should().Be(command);
        }

        [Test]
        public void TestBuildWithChildren()
        {
            LiteralArgumentBuilder<object> builder = new LiteralArgumentBuilder<object>("foo");

            builder.Then(r => r.Argument("bar", Arguments.Integer()));
            builder.Then(r => r.Argument("baz", Arguments.Integer()));
            var node = builder.Build();

            node.Children.Should().HaveCount(2);
        }
    }
}
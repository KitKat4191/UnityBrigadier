// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Brigadier.NET.Builder;
using Brigadier.NET.Tree;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Brigadier.NET.Tests.builder
{
    public class ArgumentBuilderTest
    {
        [Test]
        public void TestArguments()
        {
            TestableArgumentBuilder<object> builder = new TestableArgumentBuilder<object>();

            var argument = RequiredArgumentBuilder<object, int>.RequiredArgument("bar", Arguments.Integer());

            builder.Then(argument);

            builder.Arguments.Should().HaveCount(1);
            builder.Arguments.Should().ContainSingle().Which.Should().Be(argument.Build());
        }

        [Test]
        public void TestRedirect()
        {
            TestableArgumentBuilder<object> builder = new TestableArgumentBuilder<object>();

            var target = Substitute.For<CommandNode<object>>(null, null, null, null, false);
            builder.Redirect(target);
            builder.RedirectTarget.Should().Be(target);
        }

        [Test]
        public void testRedirect_withChild()
        {
            TestableArgumentBuilder<object> builder = new TestableArgumentBuilder<object>();

            var target = Substitute.For<CommandNode<object>>(null, null, null, null, false);
            builder.Then(r => r.Literal("foot"));
            builder.Invoking(b => b.Redirect(target))
                .Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void testThen_withRedirect()
        {
            TestableArgumentBuilder<object> builder = new TestableArgumentBuilder<object>();

            var target = Substitute.For<CommandNode<object>>(null, null, null, null, false);

            builder.Redirect(target);
            builder.Invoking(b => b.Then(r => r.Literal("foot")))
                .Should().Throw<InvalidOperationException>();
        }

        internal class
            TestableArgumentBuilder<TSource> : ArgumentBuilder<TSource, TestableArgumentBuilder<TSource>,
                CommandNode<TSource>>
        {
            public override CommandNode<TSource> Build()
            {
                throw new NotImplementedException();
            }
        }
    }
}
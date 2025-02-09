// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.ArgumentTypes;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Brigadier.NET.Tests.arguments
{
    public class BoolArgumentTypeTest
    {
        [Test]
        public void Parse()
        {
            BoolArgumentType type = Arguments.Bool();
            var reader = Substitute.For<IStringReader>();
            reader.ReadBoolean().Returns(true);
            type.Parse(reader).Should().Be(true);

            reader.Received().ReadBoolean();
        }
    }
}
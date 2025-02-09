// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.ArgumentTypes;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Brigadier.NET.Tests.arguments
{
	public class StringArgumentTypeTest {

		[Test]
		public void TestParseWord()
		{
			var reader = Substitute.For<IStringReader>();
			reader.ReadUnquotedString().Returns("hello");
			Arguments.Word().Parse(reader).Should().BeEquivalentTo("hello");

			reader.Received().ReadUnquotedString();
		}

		[Test]
		public void TestParseString(){
			var reader = Substitute.For<IStringReader>();
			reader.ReadString().Returns("hello world");
			Arguments.String().Parse(reader).Should().BeEquivalentTo("hello world");
			reader.Received().ReadString();
		}

		[Test]
		public void TestParseGreedyString(){
			var reader = new StringReader("Hello world! This is a test.");
			Arguments.GreedyString().Parse(reader).Should().BeEquivalentTo("Hello world! This is a test.");
			reader.CanRead().Should().Be(false);
		}

		[Test]
		public void TestToString(){
			Arguments.String().ToString().Should().BeEquivalentTo("string()");
		}

		[Test]
		public void testEscapeIfRequired_notRequired(){
			StringArgumentType.EscapeIfRequired("hello").Should().BeEquivalentTo("hello");
			StringArgumentType.EscapeIfRequired("").Should().BeEquivalentTo("");
		}

		[Test]
		public void testEscapeIfRequired_multipleWords(){
			StringArgumentType.EscapeIfRequired("hello world").Should().BeEquivalentTo("\"hello world\"");
		}

		[Test]
		public void testEscapeIfRequired_quote(){
			StringArgumentType.EscapeIfRequired("hello \"world\"!").Should().BeEquivalentTo("\"hello \\\"world\\\"!\"");
		}

		[Test]
		public void testEscapeIfRequired_escapes(){
			StringArgumentType.EscapeIfRequired("\\").Should().BeEquivalentTo("\"\\\\\"");
		}

		[Test]
		public void testEscapeIfRequired_singleQuote(){
			StringArgumentType.EscapeIfRequired("\"").Should().BeEquivalentTo("\"\\\"\"");
		}
	}
}
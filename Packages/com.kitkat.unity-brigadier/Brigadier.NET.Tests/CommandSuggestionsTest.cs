// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Brigadier.NET.Tests
{
    public class CommandSuggestionsTest
    {
        private readonly object _source = Substitute.For<object>();

        private CommandDispatcher<object> GetSubject => new CommandDispatcher<object>();

        private async Task TestSuggestions(CommandDispatcher<object> subject, string contents, int cursor,
            StringRange range, params string[] suggestions)
        {
            var result = await subject.GetCompletionSuggestions(subject.Parse(contents, _source), cursor);
            result.Range.Should().BeEquivalentTo(range);

            var expected = new List<Suggestion.Suggestion>();
            foreach (var suggestion in suggestions)
            {
                expected.Add(new Suggestion.Suggestion(range, suggestion));
            }

            result.List.Should().BeEquivalentTo(expected);
        }

        private static StringReader InputWithOffset(string input, int offset)
        {
            var result = new StringReader(input)
            {
                Cursor = offset
            };
            return result;
        }

        [Test]
        public async Task getCompletionSuggestions_rootCommands()
        {
            CommandDispatcher<object> subject = GetSubject;

            subject.Register(r => r.Literal("foo"));
            subject.Register(r => r.Literal("bar"));
            subject.Register(r => r.Literal("baz"));

            var result = await subject.GetCompletionSuggestions(subject.Parse("", _source));

            result.Range.Should().BeEquivalentTo(StringRange.At(0));
            result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
            {
                new Suggestion.Suggestion(StringRange.At(0), "bar"),
                new Suggestion.Suggestion(StringRange.At(0), "baz"), new Suggestion.Suggestion(StringRange.At(0), "foo")
            });
        }

        [Test]
        public async Task getCompletionSuggestions_rootCommands_withInputOffset()
        {
            CommandDispatcher<object> subject = GetSubject;

            subject.Register(r => r.Literal("foo"));
            subject.Register(r => r.Literal("bar"));
            subject.Register(r => r.Literal("baz"));

            var result = await subject.GetCompletionSuggestions(subject.Parse(InputWithOffset("OOO", 3), _source));

            result.Range.Should().BeEquivalentTo(StringRange.At(3));
            result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
            {
                new Suggestion.Suggestion(StringRange.At(3), "bar"),
                new Suggestion.Suggestion(StringRange.At(3), "baz"), new Suggestion.Suggestion(StringRange.At(3), "foo")
            });
        }

        [Test]
        public async Task getCompletionSuggestions_rootCommands_partial()
        {
            CommandDispatcher<object> subject = GetSubject;

            subject.Register(r => r.Literal("foo"));
            subject.Register(r => r.Literal("bar"));
            subject.Register(r => r.Literal("baz"));

            var result = await subject.GetCompletionSuggestions(subject.Parse("b", _source));

            result.Range.Should().BeEquivalentTo(StringRange.Between(0, 1));
            result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
            {
                new Suggestion.Suggestion(StringRange.Between(0, 1), "bar"),
                new Suggestion.Suggestion(StringRange.Between(0, 1), "baz")
            });
        }

        [Test]
        public async Task getCompletionSuggestions_rootCommands_partial_withInputOffset()
        {
            CommandDispatcher<object> subject = GetSubject;

            subject.Register(r => r.Literal("foo"));
            subject.Register(r => r.Literal("bar"));
            subject.Register(r => r.Literal("baz"));

            var result = await subject.GetCompletionSuggestions(subject.Parse(InputWithOffset("Zb", 1), _source));

            result.Range.Should().BeEquivalentTo(StringRange.Between(1, 2));
            result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
            {
                new Suggestion.Suggestion(StringRange.Between(1, 2), "bar"),
                new Suggestion.Suggestion(StringRange.Between(1, 2), "baz")
            });
        }

        [Test]
        public async Task getCompletionSuggestions_SubCommands()
        {
            CommandDispatcher<object> subject = GetSubject;

            subject.Register(r =>
                r.Literal("parent")
                    .Then(r.Literal("foo"))
                    .Then(r.Literal("bar"))
                    .Then(r.Literal("baz"))
            );

            var result = await subject.GetCompletionSuggestions(subject.Parse("parent ", _source));

            result.Range.Should().BeEquivalentTo(StringRange.At(7));
            result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
            {
                new Suggestion.Suggestion(StringRange.At(7), "bar"),
                new Suggestion.Suggestion(StringRange.At(7), "baz"), new Suggestion.Suggestion(StringRange.At(7), "foo")
            });
        }

        [Test]
        public async Task getCompletionSuggestions_movingCursor_SubCommands()
        {
            CommandDispatcher<object> subject = GetSubject;

            subject.Register(r =>
                r.Literal("parent_one")
                    .Then(r.Literal("faz"))
                    .Then(r.Literal("fbz"))
                    .Then(r.Literal("gaz"))
            );

            subject.Register(r =>
                r.Literal("parent_two")
            );

            await TestSuggestions(subject, "parent_one faz ", 0, StringRange.At(0), "parent_one", "parent_two");
            await TestSuggestions(subject, "parent_one faz ", 1, StringRange.Between(0, 1), "parent_one",
                "parent_two");
            await TestSuggestions(subject, "parent_one faz ", 7, StringRange.Between(0, 7), "parent_one",
                "parent_two");
            await TestSuggestions(subject, "parent_one faz ", 8, StringRange.Between(0, 8), "parent_one");
            await TestSuggestions(subject, "parent_one faz ", 10, StringRange.At(0));
            await TestSuggestions(subject, "parent_one faz ", 11, StringRange.At(11), "faz", "fbz", "gaz");
            await TestSuggestions(subject, "parent_one faz ", 12, StringRange.Between(11, 12), "faz", "fbz");
            await TestSuggestions(subject, "parent_one faz ", 13, StringRange.Between(11, 13), "faz");
            await TestSuggestions(subject, "parent_one faz ", 14, StringRange.At(0));
            await TestSuggestions(subject, "parent_one faz ", 15, StringRange.At(0));
        }

        [Test]
        public async Task getCompletionSuggestions_SubCommands_partial()
        {
            CommandDispatcher<object> subject = GetSubject;

            subject.Register(r =>
                r.Literal("parent")
                    .Then(r.Literal("foo"))
                    .Then(r.Literal("bar"))
                    .Then(r.Literal("baz"))
            );

            var parse = subject.Parse("parent b", _source);
            var result = await subject.GetCompletionSuggestions(parse);

            result.Range.Should().BeEquivalentTo(StringRange.Between(7, 8));
            result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
            {
                new Suggestion.Suggestion(StringRange.Between(7, 8), "bar"),
                new Suggestion.Suggestion(StringRange.Between(7, 8), "baz")
            });
        }

        [Test]
        public async Task getCompletionSuggestions_SubCommands_partial_withInputOffset()
        {
            CommandDispatcher<object> subject = GetSubject;

            subject.Register(r =>
                r.Literal("parent")
                    .Then(r.Literal("foo"))
                    .Then(r.Literal("bar"))
                    .Then(r.Literal("baz"))
            );

            var parse = subject.Parse(InputWithOffset("junk parent b", 5), _source);
            var result = await subject.GetCompletionSuggestions(parse);

            result.Range.Should().BeEquivalentTo(StringRange.Between(12, 13));
            result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
            {
                new Suggestion.Suggestion(StringRange.Between(12, 13), "bar"),
                new Suggestion.Suggestion(StringRange.Between(12, 13), "baz")
            });
        }

        [Test]
        public async Task getCompletionSuggestions_redirect()
        {
            CommandDispatcher<object> subject = GetSubject;

            var actual = subject.Register(r => r.Literal("actual").Then(r.Literal("sub")));
            subject.Register(r => r.Literal("redirect").Redirect(actual));

            var parse = subject.Parse("redirect ", _source);
            var result = await subject.GetCompletionSuggestions(parse);

            result.Range.Should().BeEquivalentTo(StringRange.At(9));
            result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
                { new Suggestion.Suggestion(StringRange.At(9), "sub") });
        }

        [Test]
        public async Task getCompletionSuggestions_redirectPartial()
        {
            CommandDispatcher<object> subject = GetSubject;

            var actual = subject.Register(r => r.Literal("actual").Then(r.Literal("sub")));
            subject.Register(r => r.Literal("redirect").Redirect(actual));

            var parse = subject.Parse("redirect s", _source);
            var result = await subject.GetCompletionSuggestions(parse);

            result.Range.Should().BeEquivalentTo(StringRange.Between(9, 10));
            result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
                { new Suggestion.Suggestion(StringRange.Between(9, 10), "sub") });
        }

        [Test]
        public async Task getCompletionSuggestions_movingCursor_redirect()
        {
            CommandDispatcher<object> subject = GetSubject;

            var actualOne = subject.Register(r => r.Literal("actual_one")
                .Then(r.Literal("faz"))
                .Then(r.Literal("fbz"))
                .Then(r.Literal("gaz"))
            );

            subject.Register(r => r.Literal("actual_two"));

            subject.Register(r => r.Literal("redirect_one").Redirect(actualOne));
            subject.Register(r => r.Literal("redirect_two").Redirect(actualOne));

            await TestSuggestions(subject, "redirect_one faz ", 0, StringRange.At(0), "actual_one", "actual_two",
                "redirect_one", "redirect_two");
            await TestSuggestions(subject, "redirect_one faz ", 9, StringRange.Between(0, 9), "redirect_one",
                "redirect_two");
            await TestSuggestions(subject, "redirect_one faz ", 10, StringRange.Between(0, 10), "redirect_one");
            await TestSuggestions(subject, "redirect_one faz ", 12, StringRange.At(0));
            await TestSuggestions(subject, "redirect_one faz ", 13, StringRange.At(13), "faz", "fbz", "gaz");
            await TestSuggestions(subject, "redirect_one faz ", 14, StringRange.Between(13, 14), "faz", "fbz");
            await TestSuggestions(subject, "redirect_one faz ", 15, StringRange.Between(13, 15), "faz");
            await TestSuggestions(subject, "redirect_one faz ", 16, StringRange.At(0));
            await TestSuggestions(subject, "redirect_one faz ", 17, StringRange.At(0));
        }

        [Test]
        public async Task getCompletionSuggestions_redirectPartial_withInputOffset()
        {
            CommandDispatcher<object> subject = GetSubject;

            var actual = subject.Register(r => r.Literal("actual").Then(r.Literal("sub")));
            subject.Register(r => r.Literal("redirect").Redirect(actual));

            var parse = subject.Parse(InputWithOffset("/redirect s", 1), _source);
            var result = await subject.GetCompletionSuggestions(parse);

            result.Range.Should().BeEquivalentTo(StringRange.Between(10, 11));
            result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
                { new Suggestion.Suggestion(StringRange.Between(10, 11), "sub") });
        }

        [Test]
        public async Task getCompletionSuggestions_redirect_lots()
        {
            CommandDispatcher<object> subject = GetSubject;

            var loop = subject.Register(r => r.Literal("redirect"));
            subject.Register(r =>
                r.Literal("redirect")
                    .Then(
                        r.Literal("loop")
                            .Then(
                                r.Argument("loop", Arguments.Integer())
                                    .Redirect(loop)
                            )
                    )
            );

            var result =
                await subject.GetCompletionSuggestions(subject.Parse("redirect loop 1 loop 02 loop 003 ", _source));

            result.Range.Should().BeEquivalentTo(StringRange.At(33));
            result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
                { new Suggestion.Suggestion(StringRange.At(33), "loop") });
        }

        [Test]
        public async Task getCompletionSuggestions_execute_simulation()
        {
            CommandDispatcher<object> subject = GetSubject;

            var execute = subject.Register(r => r.Literal("execute"));
            subject.Register(r =>
                r.Literal("execute")
                    .Then(
                        r.Literal("as")
                            .Then(
                                r.Argument("name", Arguments.Word())
                                    .Redirect(execute)
                            )
                    )
                    .Then(
                        r.Literal("store")
                            .Then(
                                r.Argument("name", Arguments.Word())
                                    .Redirect(execute)
                            )
                    )
                    .Then(
                        r.Literal("run")
                            .Executes(e => 0)
                    )
            );

            var parse = subject.Parse("execute as Dinnerbone as", _source);
            var result = await subject.GetCompletionSuggestions(parse);

            result.IsEmpty().Should().Be(true);
        }

        [Test]
        public async Task getCompletionSuggestions_execute_simulation_partial()
        {
            CommandDispatcher<object> subject = GetSubject;

            var execute = subject.Register(r => r.Literal("execute"));
            subject.Register(r =>
                r.Literal("execute")
                    .Then(
                        r.Literal("as")
                            .Then(r.Literal("bar").Redirect(execute))
                            .Then(r.Literal("baz").Redirect(execute))
                    )
                    .Then(
                        r.Literal("store")
                            .Then(
                                r.Argument("name", Arguments.Word())
                                    .Redirect(execute)
                            )
                    )
                    .Then(
                        r.Literal("run").Executes(e => 0)
                    )
            );

            var parse = subject.Parse("execute as bar as ", _source);
            var result = await subject.GetCompletionSuggestions(parse);

            result.Range.Should().BeEquivalentTo(StringRange.At(18));
            result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
            {
                new Suggestion.Suggestion(StringRange.At(18), "bar"),
                new Suggestion.Suggestion(StringRange.At(18), "baz")
            });
        }
    }
}
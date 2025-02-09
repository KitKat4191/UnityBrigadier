// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Brigadier.NET.Context;
using Brigadier.NET.Suggestion;
using FluentAssertions;
using NUnit.Framework;

namespace Brigadier.NET.Tests.suggestion
{
    public class SuggestionsBuilderTest
    {
        private SuggestionsBuilder GetBuilder => new SuggestionsBuilder("Hello w", 6);

        [Test]
        public void suggest_appends()
        {
            SuggestionsBuilder _builder = GetBuilder;

            var result = _builder.Suggest("world!").Build();
            result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
                { new Suggestion.Suggestion(StringRange.Between(6, 7), "world!") });
            result.Range.Should().BeEquivalentTo(StringRange.Between(6, 7));
            result.IsEmpty().Should().Be(false);
        }

        [Test]
        public void suggest_replaces()
        {
            SuggestionsBuilder _builder = GetBuilder;

            var result = _builder.Suggest("everybody").Build();
            result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
                { new Suggestion.Suggestion(StringRange.Between(6, 7), "everybody") });
            result.Range.Should().BeEquivalentTo(StringRange.Between(6, 7));
            result.IsEmpty().Should().Be(false);
        }

        [Test]
        public void suggest_noop()
        {
            SuggestionsBuilder _builder = GetBuilder;

            var result = _builder.Suggest("w").Build();
            result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>());
            result.IsEmpty().Should().Be(true);
        }

        [Test]
        public void suggest_multiple()
        {
            SuggestionsBuilder _builder = GetBuilder;

            var result = _builder.Suggest("world!").Suggest("everybody").Suggest("weekend").Build();
            result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
            {
                new Suggestion.Suggestion(StringRange.Between(6, 7), "everybody"),
                new Suggestion.Suggestion(StringRange.Between(6, 7), "weekend"),
                new Suggestion.Suggestion(StringRange.Between(6, 7), "world!")
            });
            result.Range.Should().BeEquivalentTo(StringRange.Between(6, 7));
            result.IsEmpty().Should().Be(false);
        }

        [Test]
        public void Restart()
        {
            SuggestionsBuilder _builder = GetBuilder;

            _builder.Suggest("won't be included in restart");
            var other = _builder.Restart();
            other.Should().NotBe(_builder);
            other.Input.Should().BeEquivalentTo(_builder.Input);
            other.Start.Should().Be(_builder.Start);
            other.Remaining.Should().BeEquivalentTo(_builder.Remaining);
        }

        [Test]
        public void sort_alphabetical()
        {
            SuggestionsBuilder _builder = GetBuilder;

            var result = _builder.Suggest("2").Suggest("4").Suggest("6").Suggest("8").Suggest("30").Suggest("32")
                .Build();
            var actual = result.List.Select(s => s.Text).ToList();
            actual.Should().BeEquivalentTo(new List<string> { "2", "30", "32", "4", "6", "8" });
        }

        [Test]
        public void sort_numerical()
        {
            SuggestionsBuilder _builder = GetBuilder;

            var result = _builder.Suggest(2).Suggest(4).Suggest(6).Suggest(8).Suggest(30).Suggest(32).Build();
            var actual = result.List.Select(s => s.Text).ToList();
            actual.Should().BeEquivalentTo(new List<string> { "2", "4", "6", "8", "30", "32" });
        }

        [Test]
        public void sort_mixed()
        {
            SuggestionsBuilder _builder = GetBuilder;

            var result = _builder.Suggest("11").Suggest("22").Suggest("33").Suggest("a").Suggest("b").Suggest("c")
                .Suggest(2).Suggest(4).Suggest(6).Suggest(8).Suggest(30).Suggest(32).Suggest("3a").Suggest("a3")
                .Build();
            var actual = result.List.Select(s => s.Text).ToList();
            actual.Should().BeEquivalentTo(new List<string>
                { "11", "2", "22", "33", "3a", "4", "6", "8", "30", "32", "a", "a3", "b", "c" });
        }
    }
}
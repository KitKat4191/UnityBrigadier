// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Brigadier.NET.Builder;
using Brigadier.NET.Tree;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Brigadier.NET.Tests
{
    public class CommandDispatcherUsagesTest
    {
        private readonly object _source = Substitute.For<object>();
        private readonly Command<object> _command = Substitute.For<Command<object>>();

        private CommandDispatcher<object> GetSubject
        {
            get
            {
                var subject = new CommandDispatcher<object>();

                subject.Register(
                    r => r.Literal("a")
                        .Then(
                            r.Literal("1")
                                .Then(r.Literal("i").Executes(_command))
                                .Then(r.Literal("ii").Executes(_command))
                        )
                        .Then(
                            r.Literal("2")
                                .Then(r.Literal("i").Executes(_command))
                                .Then(r.Literal("ii").Executes(_command))
                        )
                );
                subject.Register(r => r.Literal("b").Then(r.Literal("1").Executes(_command)));
                subject.Register(r => r.Literal("c").Executes(_command));
                subject.Register(r => r.Literal("d").Requires(s => false).Executes(_command));
                subject.Register(r => r.Literal("e")
                    .Executes(_command)
                    .Then(
                        r.Literal("1")
                            .Executes(_command)
                            .Then(r.Literal("i").Executes(_command))
                            .Then(r.Literal("ii").Executes(_command))
                    )
                );
                subject.Register(r =>
                    r.Literal("f")
                        .Then(
                            r.Literal("1")
                                .Then(r.Literal("i").Executes(_command))
                                .Then(r.Literal("ii").Executes(_command).Requires(s => false))
                        )
                        .Then(
                            r.Literal("2")
                                .Then(r.Literal("i").Executes(_command).Requires(s => false))
                                .Then(r.Literal("ii").Executes(_command))
                        )
                );
                subject.Register(r =>
                    r.Literal("g")
                        .Executes(_command)
                        .Then(r.Literal("1").Then(r.Literal("i").Executes(_command)))
                );
                subject.Register(r =>
                    r.Literal("h")
                        .Executes(_command)
                        .Then(r.Literal("1").Then(r.Literal("i").Executes(_command)))
                        .Then(r.Literal("2").Then(r.Literal("i").Then(r.Literal("ii").Executes(_command))))
                        .Then(r.Literal("3").Executes(_command))
                );
                subject.Register(r =>
                    r.Literal("i")
                        .Executes(_command)
                        .Then(r.Literal("1").Executes(_command))
                        .Then(r.Literal("2").Executes(_command))
                );
                subject.Register(r =>
                    r.Literal("j")
                        .Redirect(subject.GetRoot())
                );
                subject.Register(r =>
                    r.Literal("k")
                        .Redirect(Get(subject, "h"))
                );

                return subject;
            }
        }

        private CommandNode<object> Get(CommandDispatcher<object> subject, string command)
        {
            return subject.Parse(command, _source).Context.Nodes.Last().Node;
        }

        private CommandNode<object> Get(CommandDispatcher<object> subject, StringReader command)
        {
            return subject.Parse(command, _source).Context.Nodes.Last().Node;
        }

        [Test]
        public void testAllUsage_noCommands()
        {
            CommandDispatcher<object> subject = GetSubject;

            subject = new CommandDispatcher<object>();
            var results = subject.GetAllUsage(subject.GetRoot(), _source, true);
            results.Should().BeEmpty();
        }

        [Test]
        public void testSmartUsage_noCommands()
        {
            CommandDispatcher<object> subject = GetSubject;

            subject = new CommandDispatcher<object>();
            var results = subject.GetSmartUsage(subject.GetRoot(), _source);
            results.Should().BeEmpty();
        }

        [Test]
        public void testAllUsage_root()
        {
            CommandDispatcher<object> subject = GetSubject;

            var results = subject.GetAllUsage(subject.GetRoot(), _source, true);
            results.Should().ContainInOrder(
                "a 1 i",
                "a 1 ii",
                "a 2 i",
                "a 2 ii",
                "b 1",
                "c",
                "e",
                "e 1",
                "e 1 i",
                "e 1 ii",
                "f 1 i",
                "f 2 ii",
                "g",
                "g 1 i",
                "h",
                "h 1 i",
                "h 2 i ii",
                "h 3",
                "i",
                "i 1",
                "i 2",
                "j ...",
                "k -> h"
            );
        }

        [Test]
        public void testSmartUsage_root()
        {
            CommandDispatcher<object> subject = GetSubject;

            var results = subject.GetSmartUsage(subject.GetRoot(), _source);
            results.Should().Contain(new Dictionary<CommandNode<object>, string>
            {
                { Get(subject, "a"), "a (1|2)" },
                { Get(subject, "b"), "b 1" },
                { Get(subject, "c"), "c" },
                { Get(subject, "e"), "e [1]" },
                { Get(subject, "f"), "f (1|2)" },
                { Get(subject, "g"), "g [1]" },
                { Get(subject, "h"), "h [1|2|3]" },
                { Get(subject, "i"), "i [1|2]" },
                { Get(subject, "j"), "j ..." },
                { Get(subject, "k"), "k -> h" }
            });
        }

        [Test]
        public void testSmartUsage_h()
        {
            CommandDispatcher<object> subject = GetSubject;

            var results = subject.GetSmartUsage(Get(subject, "h"), _source);
            results.Should().Contain(new Dictionary<CommandNode<object>, string>
            {
                { Get(subject, "h 1"), "[1] i" },
                { Get(subject, "h 2"), "[2] i ii" },
                { Get(subject, "h 3"), "[3]" }
            });
        }

        [Test]
        public void testSmartUsage_offsetH()
        {
            CommandDispatcher<object> subject = GetSubject;

            var offsetH = new StringReader("/|/|/h")
            {
                Cursor = 5
            };

            var results = subject.GetSmartUsage(Get(subject, offsetH), _source);
            results.Should().Contain(new Dictionary<CommandNode<object>, string>
            {
                { Get(subject, "h 1"), "[1] i" },
                { Get(subject, "h 2"), "[2] i ii" },
                { Get(subject, "h 3"), "[3]" }
            });
        }
    }
}
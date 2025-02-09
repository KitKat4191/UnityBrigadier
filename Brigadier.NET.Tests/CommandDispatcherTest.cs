using System.Collections.Generic;
using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using static Brigadier.NET.Arguments;

namespace Brigadier.NET.Tests
{
    public class CommandDispatcherTest
    {
        private readonly object _source = Substitute.For<object>();

        private CommandDispatcher<object> GetSubject => new CommandDispatcher<object>();

        private Command<object> GetCommand
        {
            get
            {
                var command = Substitute.For<Command<object>>();
                command.Invoke(Arg.Any<CommandContext<object>>()).Returns(42);

                return command;
            }
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
        public void TestCreateAndExecuteCommand()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            _subject.Register(r => r.Literal("foo").Executes(_command));

            _subject.Execute("foo", _source).Should().Be(42);
            _command.Received().Invoke(Arg.Any<CommandContext<object>>());
        }


        [Test]
        public void TestCreateAndExecuteOffsetCommand()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            _subject.Register(r => r.Literal("foo").Executes(_command));

            _subject.Execute(InputWithOffset("/foo", 1), _source).Should().Be(42);
            _command.Received().Invoke(Arg.Any<CommandContext<object>>());
        }


        [Test]
        public void TestCreateAndMergeCommands()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            _subject.Register(r => r.Literal("base").Then(r.Literal("foo").Executes(_command)));
            _subject.Register(r => r.Literal("base").Then(r.Literal("bar").Executes(_command)));

            _subject.Execute("base foo", _source).Should().Be(42);
            _subject.Execute("base bar", _source).Should().Be(42);

            _command.Received(2).Invoke(Arg.Any<CommandContext<object>>());
        }

        [Test]
        public void TestExecuteUnknownCommand()
        {
            CommandDispatcher<object> _subject = GetSubject;

            _subject.Register(r => r.Literal("bar"));
            _subject.Register(r => r.Literal("baz"));

            _subject.Invoking(s => s.Execute("foo", _source)).Should().Throw<CommandSyntaxException>()
                .Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownCommand())
                .Where(ex => ex.Cursor == 0);
        }

        [Test]
        public void TestExecuteImpermissibleCommand()
        {
            CommandDispatcher<object> _subject = GetSubject;

            _subject.Register(r => r.Literal("foo").Requires(s => false));

            _subject.Invoking(s => s.Execute("foo", _source)).Should().Throw<CommandSyntaxException>()
                .Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownCommand())
                .Where(ex => ex.Cursor == 0);
        }

        [Test]
        public void TestExecuteEmptyCommand()
        {
            CommandDispatcher<object> _subject = GetSubject;

            _subject.Register(r => r.Literal(""));

            _subject.Invoking(s => s.Execute("", _source)).Should().Throw<CommandSyntaxException>()
                .Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownCommand())
                .Where(ex => ex.Cursor == 0);
        }

        [Test]
        public void TestExecuteUnknownSubCommand()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            _subject.Register(r => r.Literal("foo").Executes(_command));

            _subject.Invoking(s => s.Execute("foo bar", _source)).Should().Throw<CommandSyntaxException>()
                .Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownArgument())
                .Where(ex => ex.Cursor == 4);
        }

        [Test]
        public void TestExecuteIncorrectLiteral()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            _subject.Register(r => r.Literal("foo").Executes(_command).Then(r.Literal("bar")));
            _subject.Invoking(s => s.Execute("foo baz", _source)).Should().Throw<CommandSyntaxException>()
                .Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownArgument())
                .Where(ex => ex.Cursor == 4);
        }

        [Test]
        public void TestExecuteAmbiguousIncorrectArgument()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            _subject.Register(r =>
                r.Literal("foo").Executes(_command)
                    .Then(r.Literal("bar"))
                    .Then(r.Literal("baz"))
            );
            _subject.Invoking(s => s.Execute("foo unknown", _source)).Should().Throw<CommandSyntaxException>()
                .Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownArgument())
                .Where(ex => ex.Cursor == 4);
        }


        [Test]
        public void TestExecuteSubCommand()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            var subCommand = Substitute.For<Command<object>>();
            subCommand.Invoke(Arg.Any<CommandContext<object>>()).Returns(100);

            _subject.Register(r =>
                r.Literal("foo")
                    .Then(r.Literal("a"))
                    .Then(r.Literal("=").Executes(subCommand))
                    .Then(r.Literal("c"))
                    .Executes(_command));

            _subject.Execute("foo =", _source).Should().Be(100);
            subCommand.Received().Invoke(Arg.Any<CommandContext<object>>());
        }


        [Test]
        public void TestParseIncompleteLiteral()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            _subject.Register(r => r.Literal("foo").Then(r.Literal("bar").Executes(_command)));

            var parse = _subject.Parse("foo ", _source);
            parse.Reader.Remaining.Should().BeEquivalentTo(" ");
            parse.Context.Nodes.Count.Should().Be(1);
        }


        [Test]
        public void TestParseIncompleteArgument()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            _subject.Register(r => r.Literal("foo").Then(r.Argument("bar", Integer()).Executes(_command)));

            var parse = _subject.Parse("foo ", _source);
            parse.Reader.Remaining.Should().BeEquivalentTo(" ");
            parse.Context.Nodes.Count.Should().Be(1);
        }

        [Test]
        public void TestExecuteAmbiguousParentSubCommand()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            var subCommand = Substitute.For<Command<object>>();
            subCommand.Invoke(Arg.Any<CommandContext<object>>()).Returns(100);

            _subject.Register(r =>
                r.Literal("test")
                    .Then(
                        r.Argument("incorrect", Integer()).Executes(_command)
                    )
                    .Then(
                        r.Argument("right", Integer())
                            .Then(
                                r.Argument("sub", Integer()).Executes(subCommand)
                            )
                    )
            );

            _subject.Execute("test 1 2", _source).Should().Be(100);
            subCommand.Received().Invoke(Arg.Any<CommandContext<object>>());
            _command.DidNotReceive().Invoke(Arg.Any<CommandContext<object>>());
        }

        [Test]
        public void TestExecuteAmbiguousParentSubCommandViaRedirect()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            var subCommand = Substitute.For<Command<object>>();
            subCommand.Invoke(Arg.Any<CommandContext<object>>()).Returns(100);

            var real = _subject.Register(r =>
                r.Literal("test")
                    .Then(
                        r.Argument("incorrect", Integer())
                            .Executes(_command)
                    )
                    .Then(
                        r.Argument("right", Integer())
                            .Then(
                                r.Argument("sub", Integer())
                                    .Executes(subCommand)
                            )
                    )
            );

            _subject.Register(r => r.Literal("redirect").Redirect(real));

            _subject.Execute("redirect 1 2", _source).Should().Be(100);
            subCommand.Received().Invoke(Arg.Any<CommandContext<object>>());
            _command.DidNotReceive().Invoke(Arg.Any<CommandContext<object>>());
        }


        [Test]
        public void TestExecuteRedirectedMultipleTimes()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            var concreteNode = _subject.Register(r => r.Literal("actual").Executes(_command));
            var redirectNode = _subject.Register(r => r.Literal("redirected").Redirect(_subject.GetRoot()));

            var input = "redirected redirected actual";

            var parse = _subject.Parse(input, _source);
            parse.Context.Range.Get(input).Should().BeEquivalentTo("redirected");
            parse.Context.Nodes.Count.Should().Be(1);
            parse.Context.RootNode.Should().Be(_subject.GetRoot());
            parse.Context.Nodes[0].Range.Should().BeEquivalentTo(parse.Context.Range);
            parse.Context.Nodes[0].Node.Should().Be(redirectNode);

            var child1 = parse.Context.Child;
            child1.Should().NotBeNull();
            child1.Range.Get(input).Should().BeEquivalentTo("redirected");
            child1.Nodes.Count.Should().Be(1);
            child1.RootNode.Should().Be(_subject.GetRoot());
            child1.Nodes[0].Range.Should().BeEquivalentTo(child1.Range);
            child1.Nodes[0].Node.Should().Be(redirectNode);

            var child2 = child1.Child;
            child2.Should().NotBeNull();
            child2.Range.Get(input).Should().BeEquivalentTo("actual");
            child2.Nodes.Count.Should().Be(1);
            child2.RootNode.Should().Be(_subject.GetRoot());
            child2.Nodes[0].Range.Should().BeEquivalentTo(child2.Range);
            child2.Nodes[0].Node.Should().Be(concreteNode);

            _subject.Execute(parse).Should().Be(42);
            _command.Received().Invoke(Arg.Any<CommandContext<object>>());
        }


        [Test]
        public void TestExecuteRedirected()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            var modifier = Substitute.For<RedirectModifier<object>>();
            var source1 = new object();
            var source2 = new object();
            modifier.Invoke(Arg.Is<CommandContext<object>>(s => s.Source == _source))
                .Returns(new[] { source1, source2 });

            var concreteNode = _subject.Register(r => r.Literal("actual").Executes(_command));
            var redirectNode = _subject.Register(r => r.Literal("redirected").Fork(_subject.GetRoot(), modifier));

            var input = "redirected actual";
            var parse = _subject.Parse(input, _source);
            parse.Context.Range.Get(input).Should().BeEquivalentTo("redirected");
            parse.Context.Nodes.Count.Should().Be(1);
            parse.Context.RootNode.Should().BeEquivalentTo(_subject.GetRoot());
            parse.Context.Nodes[0].Range.Should().BeEquivalentTo(parse.Context.Range);
            parse.Context.Nodes[0].Node.Should().Be(redirectNode);
            parse.Context.Source.Should().Be(_source);

            var parent = parse.Context.Child;
            parent.Should().NotBeNull();
            parent.Range.Get(input).Should().BeEquivalentTo("actual");
            parent.Nodes.Count.Should().Be(1);
            parse.Context.RootNode.Should().BeEquivalentTo(_subject.GetRoot());
            parent.Nodes[0].Range.Should().BeEquivalentTo(parent.Range);
            parent.Nodes[0].Node.Should().Be(concreteNode);
            parent.Source.Should().Be(_source);

            _subject.Execute(parse).Should().Be(2);
            _command.Received(1).Invoke(Arg.Is<CommandContext<object>>(c => c.Source == source1));
            _command.Received(1).Invoke(Arg.Is<CommandContext<object>>(c => c.Source == source2));
        }

        [Test]
        public void TestExecuteOrphanedSubCommand()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            _subject.Register(r => r.Literal("foo").Then(r.Argument("bar", Integer())).Executes(_command));

            _subject.Invoking(s => s.Execute("foo 5", _source)).Should().Throw<CommandSyntaxException>()
                .Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownCommand())
                .Where(ex => ex.Cursor == 5);
        }

        [Test]
        public void testExecute_invalidOther()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            var wrongCommand = Substitute.For<Command<object>>();
            _subject.Register(r => r.Literal("w").Executes(wrongCommand));
            _subject.Register(r => r.Literal("world").Executes(_command));

            _subject.Execute("world", _source).Should().Be(42);
            wrongCommand.DidNotReceive().Invoke(Arg.Any<CommandContext<object>>());
            _command.Received().Invoke(Arg.Any<CommandContext<object>>());
        }

        [Test]
        public void parse_noSpaceSeparator()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            _subject.Register(r => r.Literal("foo").Then(r.Argument("bar", Integer()).Executes(_command)));

            _subject.Invoking(s => s.Execute("foo$", _source))
                .Should().Throw<CommandSyntaxException>()
                .Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DispatcherUnknownCommand())
                .Where(ex => ex.Cursor == 0);
        }

        [Test]
        public void TestExecuteInvalidSubCommand()
        {
            CommandDispatcher<object> _subject = GetSubject;
            Command<object> _command = GetCommand;

            _subject.Register(r => r.Literal("foo").Then(c => c.Argument("bar", Integer())).Executes(_command));

            _subject.Invoking(s => s.Execute("foo bar", _source))
                .Should().Throw<CommandSyntaxException>()
                .Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.ReaderExpectedInt())
                .Where(ex => ex.Cursor == 4);
        }

        [Test]
        public void TestGetPath()
        {
            CommandDispatcher<object> _subject = GetSubject;

            var bar = LiteralArgumentBuilder<object>.LiteralArgument("bar").Build();
            _subject.Register(r => r.Literal("foo").Then(bar));

            _subject.GetPath(bar).Should().BeEquivalentTo(new List<string> { "foo", "bar" });
        }

        [Test]
        public void TestFindNodeExists()
        {
            CommandDispatcher<object> _subject = GetSubject;

            var bar = LiteralArgumentBuilder<object>.LiteralArgument("bar").Build();
            _subject.Register(r => r.Literal("foo").Then(bar));

            _subject.FindNode(new List<string> { "foo", "bar" }).Should().Be(bar);
        }

        [Test]
        public void TestFindNodeDoesNotExist()
        {
            CommandDispatcher<object> _subject = GetSubject;

            _subject.FindNode(new List<string> { "foo", "bar" }).Should().BeNull();
        }
    }
}
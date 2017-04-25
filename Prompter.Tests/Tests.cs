using Microsoft.ServiceFabric.Actors;
using Prompter.Tests.Mocks;
using ServiceFabric.Mocks;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Prompter.Tests
{
    public class Tests
    {
        [Fact]
        public async Task Prompt_OnceAndMany_ReturnsValidReminder()
        {
            OnPrompt callback = (string n, byte[] x, TimeSpan d, TimeSpan p, Guid? c) => Task.FromResult(false);
            var service = MockActorServiceFactory.CreateActorServiceForActor<ActorFixture>(
                (svc, id) => new ActorFixture(svc, id, callback));
            var actor = service.Activate(new ActorId(Guid.NewGuid()));

            var cid = Guid.NewGuid();
            var onceName = "once thingy";
            var manyName = "manyName";
            var manyPeriod = TimeSpan.FromSeconds(5);
            var due = TimeSpan.FromSeconds(1);

            var onceResult = await actor.PromptOnce(onceName, due, cid);
            var manyResult = await actor.PromptMany(manyName, due, manyPeriod, cid);

            Assert.Equal(onceName, onceResult.Name);
            Assert.Equal(due, onceResult.DueTime);

            Assert.Equal(manyName, manyResult.Name);
            Assert.Equal(due, manyResult.DueTime);
            Assert.Equal(manyPeriod, manyResult.Period);
        }

        [Fact]
        public async Task Prompt_OnceAndMany_StoresValidReminder()
        {
            OnPrompt callback = (string n, byte[] x, TimeSpan d, TimeSpan p, Guid? c) => Task.FromResult(false);
            var service = MockActorServiceFactory.CreateActorServiceForActor<ActorFixture>(
                (svc, id) => new ActorFixture(svc, id, callback));
            var actor = service.Activate(new ActorId(Guid.NewGuid()));

            var oncePeriod = TimeSpan.FromMilliseconds(-1); // SF's way of saying 'never repeat'

            var onceResult = await actor.PromptOnce("once", TimeSpan.FromSeconds(1), Guid.NewGuid());
            var manyResult = await actor.PromptMany("many", TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), Guid.NewGuid());

            var onceOutcome = actor.GetActorReminders().First();
            var manyOutcome = actor.GetActorReminders().Last();

            Assert.Equal(onceOutcome.Name, onceResult.Name);
            Assert.Equal(onceOutcome.DueTime, onceResult.DueTime);
            Assert.Equal(onceOutcome.Period, onceResult.Period);

            Assert.Equal(manyOutcome.Name, manyResult.Name);
            Assert.Equal(manyOutcome.DueTime, manyResult.DueTime);
            Assert.Equal(manyOutcome.Period, manyResult.Period);
        }

        [Fact]
        public async Task Prompt_WithCid_CallbackReturnsCid()
        {
            var expected = Guid.NewGuid();
            Guid? result = null;
            OnPrompt callback = (string n, byte[] x, TimeSpan d, TimeSpan p, Guid? c) =>
            { result = c; return Task.FromResult(false); };
            var service = MockActorServiceFactory.CreateActorServiceForActor<ActorFixture>(
                (svc, id) => new ActorFixture(svc, id, callback));
            var actor = service.Activate(new ActorId(Guid.NewGuid()));

            var reminder = await actor.PromptOnce("yo", TimeSpan.FromSeconds(1), expected);
            await actor.ReceiveReminderAsync(reminder.Name, reminder.State, reminder.DueTime, reminder.Period);

            Assert.NotNull(result);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task Prompt_WithoutCid_CallbackDoesNotReturnCid()
        {
            Guid? result = null;
            OnPrompt callback = (string n, byte[] x, TimeSpan d, TimeSpan p, Guid? c) =>
            { result = c; return Task.FromResult(false); };
            var service = MockActorServiceFactory.CreateActorServiceForActor<ActorFixture>(
                (svc, id) => new ActorFixture(svc, id, callback));
            var actor = service.Activate(new ActorId(Guid.NewGuid()));

            var reminder = await actor.PromptOnce("yo", TimeSpan.FromSeconds(1));
            await actor.ReceiveReminderAsync(reminder.Name, reminder.State, reminder.DueTime, reminder.Period);

            Assert.Null(result);
        }

        [Fact]
        public async Task Prompt_Once_UnregistersReminder()
        {
            OnPrompt callback = (string n, byte[] x, TimeSpan d, TimeSpan p, Guid? c) => Task.FromResult(false);
            var service = MockActorServiceFactory.CreateActorServiceForActor<ActorFixture>(
                (svc, id) => new ActorFixture(svc, id, callback));
            var actor = service.Activate(new ActorId(Guid.NewGuid()));

            var cid = Guid.NewGuid();
            var name = "test reminder";
            var due = TimeSpan.FromSeconds(1);
            var never = TimeSpan.FromMilliseconds(-1); // SF's way of saying 'never repeat'

            var reminder = await actor.PromptOnce(name, due, cid);
            await actor.ReceiveReminderAsync(reminder.Name, reminder.State, reminder.DueTime, reminder.Period);

            var reminders = actor.GetActorReminders();
            Assert.Empty(reminders);
        }

        [Fact]
        public async Task Prompt_OnceAndMany_Callsback()
        {
            int callbacks = 0;
            OnPrompt callback = (string n, byte[] x, TimeSpan d, TimeSpan p, Guid? c) =>
            { callbacks++; return Task.FromResult(false); };
            var service = MockActorServiceFactory.CreateActorServiceForActor<ActorFixture>(
                (svc, id) => new ActorFixture(svc, id, callback));
            var actor = service.Activate(new ActorId(Guid.NewGuid()));

            var cid = Guid.NewGuid();
            var name = "test reminder";
            var due = TimeSpan.FromSeconds(1);
            var never = TimeSpan.FromMilliseconds(-1); // SF's way of saying 'never repeat'
            var later = TimeSpan.FromSeconds(5);

            var once = await actor.PromptOnce(name, due, cid);
            var many = await actor.PromptMany(name, due, later, cid);
            await actor.ReceiveReminderAsync(once.Name, once.State, once.DueTime, once.Period);
            await actor.ReceiveReminderAsync(many.Name, many.State, many.DueTime, many.Period);

            Assert.Equal(2, callbacks);
        }

        [Fact]
        public async Task Prompt_Bypassed_Callsback()
        {
            var expectedName = "bypassed";
            string resultName = null;
            var expectedData = new byte[] { 0x00, 0x01 };
            byte[] resultData = null;
            OnPrompt callback = (string n, byte[] x, TimeSpan d, TimeSpan p, Guid? c) =>
            { resultName = n; resultData = x; return Task.FromResult(false); };
            var service = MockActorServiceFactory.CreateActorServiceForActor<ActorFixture>(
                (svc, id) => new ActorFixture(svc, id, callback));
            var actor = service.Activate(new ActorId(Guid.NewGuid()));

            await actor.ReceiveReminderAsync(expectedName, expectedData, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            Assert.Equal(expectedName, resultName);
            Assert.Equal(expectedData, resultData);
        }

        [Fact]
        public async Task Forget_OnceAndMany_UnregistersReminders()
        {
            OnPrompt callback = (string n, byte[] x, TimeSpan d, TimeSpan p, Guid? c) => Task.FromResult(false);
            var service = MockActorServiceFactory.CreateActorServiceForActor<ActorFixture>(
                (svc, id) => new ActorFixture(svc, id, callback));
            var actor = service.Activate(new ActorId(Guid.NewGuid()));

            var cid = Guid.NewGuid();
            var due = TimeSpan.FromSeconds(1);
            var later = TimeSpan.FromSeconds(5);

            var once = await actor.PromptOnce("one", due, cid);
            var many = await actor.PromptMany("two", due, later, cid);

            Assert.True(await actor.ForgetPrompt(once.Name));
            Assert.True(await actor.ForgetPrompt(many.Name));
            Assert.Empty(actor.GetActorReminders());
            Assert.False(await actor.ForgetPrompt("non existant reminder"));
        }
    }
}

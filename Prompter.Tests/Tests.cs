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
        public async Task Prompt_OnceAndMany_CreatesValidReminders()
        {
            OnPrompt callback = (string n, byte[] x, TimeSpan d, TimeSpan p, Guid c) => Task.FromResult(false);
            var service = MockActorServiceFactory.CreateActorServiceForActor<ActorFixture>(
                (svc, id) => new ActorFixture(svc, id, callback));
            var actor = service.Activate(new ActorId(Guid.NewGuid()));

            var cid = Guid.NewGuid();
            var name = "test reminder";
            var due = TimeSpan.FromSeconds(1);
            var never = TimeSpan.FromMilliseconds(-1); // SF's way of saying 'never repeat'
            var sometime = TimeSpan.FromSeconds(5);

            var once = await actor.PromptOnce(name, due, cid);
            var many = await actor.PromptMany(name, due, sometime, cid);
            var reminders = actor.GetActorReminders();

            Assert.Equal($"prompt:once|{cid}|{name}", once.Name);
            Assert.Equal(due, once.DueTime);
            Assert.Equal(never, once.Period);

            Assert.Equal($"prompt:once|{cid}|{name}", reminders.First().Name);
            Assert.Equal(due, reminders.First().DueTime);
            Assert.Equal(never, reminders.First().Period);

            Assert.Equal($"prompt:many|{cid}|{name}", many.Name);
            Assert.Equal(due, many.DueTime);
            Assert.Equal(sometime, many.Period);

            Assert.Equal($"prompt:many|{cid}|{name}", reminders.Last().Name);
            Assert.Equal(due, reminders.Last().DueTime);
            Assert.Equal(sometime, reminders.Last().Period);
        }

        [Fact]
        public async Task Prompt_Once_UnregistersReminder()
        {
            OnPrompt callback = (string n, byte[] x, TimeSpan d, TimeSpan p, Guid c) => Task.FromResult(false);
            var service = MockActorServiceFactory.CreateActorServiceForActor<ActorFixture>(
                (svc, id) => new ActorFixture(svc, id, callback));
            var actor = service.Activate(new ActorId(Guid.NewGuid()));

            var cid = Guid.NewGuid();
            var name = "test reminder";
            var due = TimeSpan.FromSeconds(1);
            var never = TimeSpan.FromMilliseconds(-1); // SF's way of saying 'never repeat'

            var once = await actor.PromptOnce(name, due, cid);
            await actor.ReceiveReminderAsync(once.Name, once.State, once.DueTime, once.Period);
            var reminders = actor.GetActorReminders();

            Assert.Empty(reminders);
        }

        [Fact]
        public async Task Prompt_OnceAndMany_Callsback()
        {
            int callbacks = 0;
            OnPrompt callback = (string n, byte[] x, TimeSpan d, TimeSpan p, Guid c) =>
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
        public async Task Prompt_Once_InvalidName_Throws()
        {
            OnPrompt callback = (string n, byte[] x, TimeSpan d, TimeSpan p, Guid c) => Task.FromResult(false);
            var service = MockActorServiceFactory.CreateActorServiceForActor<ActorFixture>(
                (svc, id) => new ActorFixture(svc, id, callback));
            var actor = service.Activate(new ActorId(Guid.NewGuid()));

            await Assert.ThrowsAsync(typeof(ArgumentException), () => actor.PromptOnce("this | should | throw", TimeSpan.FromSeconds(1)));
        }

        [Fact]
        public async Task Prompt_Many_InvalidName_Throws()
        {
            OnPrompt callback = (string n, byte[] x, TimeSpan d, TimeSpan p, Guid c) => Task.FromResult(false);
            var service = MockActorServiceFactory.CreateActorServiceForActor<ActorFixture>(
                (svc, id) => new ActorFixture(svc, id, callback));
            var actor = service.Activate(new ActorId(Guid.NewGuid()));

            await Assert.ThrowsAsync(typeof(ArgumentException), () => actor.PromptMany("this | should | throw", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)));
        }

        [Fact]
        public async Task Forget_OnceAndMany_UnregistersReminders()
        {
            OnPrompt callback = (string n, byte[] x, TimeSpan d, TimeSpan p, Guid c) => Task.FromResult(false);
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

            Assert.True(await actor.ForgetPrompt(once.Name));
            Assert.True(await actor.ForgetPrompt(many.Name));
            Assert.Empty(actor.GetActorReminders());
            Assert.False(await actor.ForgetPrompt("non existant reminder"));
        }
    }
}

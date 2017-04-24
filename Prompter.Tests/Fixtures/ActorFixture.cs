using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prompter.Tests.Mocks
{
    internal interface IActorFixture : IActor
    {

    }

    internal sealed class ActorFixture : Actor, IActorFixture, IRemindable
    {
        private readonly Prompt _prompt;

        public ActorFixture(ActorService svc, ActorId id, OnPrompt onPrompt)
        : base(svc, id)
        {
            _prompt = new Prompt(
                onPrompt,
                GetReminder,
                RegisterReminderAsync,
                UnregisterReminderAsync,
                this);
        }

        public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period) =>
            _prompt.ReceivePrompt(reminderName, state, dueTime, period);

        public Task<IActorReminder> PromptOnce(string name, TimeSpan due, Guid? cid = null, byte[] ctx = null) =>
            _prompt.PromptOnce(name, due, cid, ctx);

        public Task<IActorReminder> PromptMany(string name, TimeSpan due, TimeSpan period, Guid? cid = null, byte[] ctx = null) =>
            _prompt.PromptMany(name, due, period, cid, ctx);

        public Task<bool> ForgetPrompt(string name) =>
            _prompt.ForgetPrompt(name);

        // e.g.
        //private Task OnPrompt(string name, byte[] context, TimeSpan due, TimeSpan period, Guid cid) =>
        //    Task.FromResult(false);
    }
}

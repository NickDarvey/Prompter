using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace Prompter
{
    public abstract class PromptableActor : Actor, IRemindable
    {
        private readonly Prompt _prompt;

        public PromptableActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
        {
            _prompt = new Prompt(OnPrompt, GetReminder, RegisterReminderAsync, UnregisterReminderAsync, this);
        }

        public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period) =>
            _prompt.ReceivePrompt(reminderName, state, dueTime, period);

        protected abstract Task OnPrompt(string name, byte[] state, TimeSpan due, TimeSpan period, Guid? cid);
    }
}

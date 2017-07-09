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

        /// <summary>
        /// Registers a prompt with the actor.
        /// This prompt will fire once and automatically clean up it's state after firing.
        /// </summary>
        /// <param name="due">
        /// A System.TimeSpan representing the amount of time to delay before firing the
        /// reminder. Specify negative one (-1) milliseconds to prevent reminder from firing.
        /// Specify zero (0) to fire the reminder immediately.
        /// </param>
        /// <param name="cid">
        /// Optionally, a correlation ID.
        /// </param>
        /// <returns>
        /// A reference to the reminder.
        /// Store this if you want to manually unregister a prompt.
        /// </returns>
        public Task<IActorReminder> PromptOnce(string name, TimeSpan due, Guid? cid = null, byte[] data = null) =>
            _prompt.PromptOnce(name, due, cid, data);

        /// <summary>
        /// Registers a prompt with the actor.
        /// </summary>
        /// <param name="due">
        /// A System.TimeSpan representing the amount of time to delay before firing the
        /// reminder. Specify negative one (-1) milliseconds to prevent reminder from firing.
        /// Specify zero (0) to fire the reminder immediately.
        /// </param>
        /// <param name="period">
        /// The time interval between firing of reminders. Specify negative one (-1) milliseconds
        /// to disable periodic firing.
        /// </param>
        /// <param name="cid">
        /// Optionally, a correlation ID.
        /// </param>
        /// <returns>
        /// A reference to the reminder.
        /// Store this if you want to manually unregister a prompt.
        /// </returns>
        public Task<IActorReminder> PromptMany(string name, TimeSpan due, TimeSpan period, Guid? cid = null, byte[] data = null) =>
            _prompt.PromptMany(name, due, period, cid, data);

        public Task<bool> ForgetPrompt(string name) =>
            _prompt.ForgetPrompt(name);
    }
}

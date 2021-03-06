// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters
{
    /// <summary>
    /// A mock adapter that can be used for unit testing bot logic.
    /// This class was created to rewrite some methods off of  <seealso cref="TestAdapter" />, to allow for Activity Ids to remain null for testing.
    /// </summary>
    public class AllowNullIdTestAdapter : TestAdapter
    {
        private bool _sendTraceActivity;
        private readonly object _conversationLock = new object();
        private readonly object _activeQueueLock = new object();
        private Queue<TaskCompletionSource<IActivity>> _queuedRequests = new Queue<TaskCompletionSource<IActivity>>();
        private int _nextId = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllowNullIdTestAdapter"/> class.
        /// </summary>
        /// <param name="conversation">A reference to the conversation to begin the adapter state with.</param>
        /// <param name="sendTraceActivity">Indicates whether the adapter should add to its <see cref="ActiveQueue"/>
        /// any trace activities generated by the bot.</param>
        public AllowNullIdTestAdapter(ConversationReference conversation = null, bool sendTraceActivity = false)
            : base(conversation, sendTraceActivity)
        { 
            _sendTraceActivity = sendTraceActivity;
            if (conversation != null)
            {
                Conversation = conversation;
            }
            else
            {
                Conversation = new ConversationReference
                {
                    ChannelId = Channels.Test,
                    ServiceUrl = "https://test.com",
                    User = new ChannelAccount("user1", "User1"),
                    Bot = new ChannelAccount("bot", "Bot"),
                    Conversation = new ConversationAccount(false, "convo1", "Conversation1")
                };
            }
        }
        
        public override Task SendTextToBotAsync(string userSays, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            return ProcessActivityAsync(MakeActivity(userSays), callback, cancellationToken);
        }

        /// <summary>
        /// Receives an activity and runs it through the middleware pipeline.
        /// </summary>
        /// <param name="activity">The activity to process.</param>
        /// <param name="callback">The bot logic to invoke.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public new async Task ProcessActivityAsync(Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken = default)
        {
            lock (_conversationLock)
            {
                // ready for next reply
                if (activity.Type == null)
                {
                    activity.Type = ActivityTypes.Message;
                }

                activity.ChannelId = Conversation.ChannelId;

                if (activity.From == null || activity.From.Id == "unknown" || activity.From.Role == RoleTypes.Bot)
                {
                    activity.From = Conversation.User;
                }

                activity.Recipient = Conversation.Bot;
                activity.Conversation = Conversation.Conversation;
                activity.ServiceUrl = Conversation.ServiceUrl;
            }

            if (activity.Timestamp == null || activity.Timestamp == default(DateTimeOffset))
            {
                activity.Timestamp = DateTimeOffset.UtcNow;
            }

            if (activity.LocalTimestamp == null || activity.LocalTimestamp == default(DateTimeOffset))
            {
                activity.LocalTimestamp = DateTimeOffset.Now;
            }

            using (var context = new TurnContext(this, activity))
            {
                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Dequeues and returns the next bot response from the <see cref="ActiveQueue"/>.
        /// </summary>
        /// <returns>The next activity in the queue; or null, if the queue is empty.</returns>
        /// <remarks>A <see cref="TestFlow"/> object calls this to get the next response from the bot.</remarks>
        public new IActivity GetNextReply()
        {
            lock (_activeQueueLock)
            {
                if (ActiveQueue.Count > 0)
                {
                    return ActiveQueue.Dequeue();
                }
            }

            return null;
        }

        /// <summary>
        /// Get the next reply async.
        /// </summary>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>activity when it's available or canceled task if it is canceled.</returns>
        public new Task<IActivity> GetNextReplyAsync(CancellationToken cancellationToken = default)
        {
            lock (_activeQueueLock)
            {
                if (!_queuedRequests.Any())
                {
                    var result = GetNextReply();
                    if (result != null)
                    {
                        return Task.FromResult(result);
                    }
                }

                var tcs = new TaskCompletionSource<IActivity>();
                cancellationToken.Register(() => tcs.SetCanceled());
                this._queuedRequests.Enqueue(tcs);
                return tcs.Task;
            }
        }

        /// <summary>
        /// Creates a message activity from text and the current conversational context.
        /// </summary>
        /// <param name="text">The message text.</param>
        /// <returns>An appropriate message activity.</returns>
        /// <remarks>A <see cref="TestFlow"/> object calls this to get a message activity
        /// appropriate to the current conversation.</remarks>
        public new Activity MakeActivity(string text = null)
        {
            Activity activity = new Activity
            {
                Type = ActivityTypes.Message,
                Locale = this.Locale,
                From = Conversation.User,
                Recipient = Conversation.Bot,
                Conversation = Conversation.Conversation,
                ServiceUrl = Conversation.ServiceUrl,
                Id = (_nextId++).ToString(),
                Text = text,
            };

            return activity;
        }

        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (activities == null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            if (activities.Length == 0)
            {
                throw new ArgumentException("Expecting one or more activities, but the array was empty.", nameof(activities));
            }

            var responses = new ResourceResponse[activities.Length];

            // NOTE: we're using for here (vs. foreach) because we want to simultaneously index into the
            // activities array to get the activity to process as well as use that index to assign
            // the response to the responses array and this is the most cost effective way to do that.
            for (var index = 0; index < activities.Length; index++)
            {
                var activity = activities[index];

                if (activity.Timestamp == null)
                {
                    activity.Timestamp = DateTime.UtcNow;
                }

                if (activity.Type == ActivityTypesEx.Delay)
                {
                    // The BotFrameworkAdapter and Console adapter implement this
                    // hack directly in the POST method. Replicating that here
                    // to keep the behavior as close as possible to facilitate
                    // more realistic tests.
                    var delayMs = (int)activity.Value;

                    await Task.Delay(delayMs).ConfigureAwait(false);
                }
                else if (activity.Type == ActivityTypes.Trace)
                {
                    if (_sendTraceActivity)
                    {
                        Enqueue(activity);
                    }
                }
                else
                {
                    Enqueue(activity);
                }

                responses[index] = new ResourceResponse(activity.Id);
            }

            return responses;
        }

        private void Enqueue(Activity activity)
        {
            lock (_activeQueueLock)
            {
                // if there are pending requests, fulfill them with the activity.
                while (_queuedRequests.Any())
                {
                    var tcs = _queuedRequests.Dequeue();
                    if (tcs.Task.IsCanceled == false)
                    {
                        tcs.SetResult(activity);
                        return;
                    }
                }

                // else we enqueue for next requester
                ActiveQueue.Enqueue(activity);
            }
        }
    }
}

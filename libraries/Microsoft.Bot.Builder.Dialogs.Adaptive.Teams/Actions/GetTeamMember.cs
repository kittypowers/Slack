// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
{
    /// <summary>
    /// Calls TeamsInfo.GetTeamMember to retrieve the list of members in a teams
    /// scoped conversation and sets the result to a memory property.
    /// </summary>
    public class GetTeamMember : Dialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.GetTeamMember";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetTeamMember"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public GetTeamMember([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; } 

        /// <summary>
        /// Gets or sets property path to put the value in.
        /// </summary>
        /// <value>
        /// Property path to put the value in.
        /// </value>
        [JsonProperty("property")]
        public StringExpression Property { get; set; }

        /// <summary>
        /// Gets or sets the expression to get the value to use for member id.
        /// </summary>
        /// <value>
        /// The expression to get the value to use for member id. Default value is turn.activity.from.id.
        /// </value>
        [JsonProperty("memberId")]
        public StringExpression MemberId { get; set; } = "=turn.activity.from.id";

        /// <summary>
        /// Gets or sets the expression to get the value to use for team id.
        /// </summary>
        /// <value>
        /// The expression to get the value to use for team id. Default value is turn.activity.channelData.team.id.
        /// </value>
        [JsonProperty("teamId")]
        public StringExpression TeamId { get; set; } = "=turn.activity.channelData.team.id";

        /// <inheritdoc/>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }
            
            if (Disabled != null && Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (dc.Context.Activity.ChannelId != Channels.Msteams)
            {
                throw new InvalidOperationException($"{Kind} works only on the Teams channel.");
            }

            string memberId = MemberId.GetValueOrNull(dc.State);
            if (string.IsNullOrEmpty(memberId))
            {
                throw new InvalidOperationException($"Missing {nameof(MemberId)} in {Kind}.");
            }

            string teamId = TeamId.GetValueOrNull(dc.State);

            var result = await TeamsInfo.GetTeamMemberAsync(dc.Context, memberId, teamId, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (Property != null)
            {
                dc.State.SetValue(Property.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}[{MemberId?.ToString() ?? string.Empty},{TeamId?.ToString() ?? string.Empty},{Property?.ToString() ?? string.Empty}]";
        }
    }
}

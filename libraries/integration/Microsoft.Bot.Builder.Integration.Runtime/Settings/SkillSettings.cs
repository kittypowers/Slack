// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Integration.Runtime.Settings
{
    /// <summary>
    /// Skill settings for the runtime.
    /// </summary>
    internal class SkillSettings
    {
        /// <summary>
        /// Gets or sets the list of application Ids that are allowed to call this bot.
        /// </summary>
        /// <value>
        /// The list of application Ids that are allowed to call this bot.
        /// </value>
        public IList<string> AllowedCallers { get; set; } = new List<string>();
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Runtime.Plugins;

namespace Microsoft.Bot.Builder.Runtime.Tests.Plugins.Implementations
{
    public class PublicBotPluginNoDefaultConstructor : IBotPlugin
    {
        public PublicBotPluginNoDefaultConstructor(bool foo)
        {
        }

        public void Load(IBotPluginLoadContext context)
        {
        }
    }
}

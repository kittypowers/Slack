// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Core.Tests
{
    internal class StartupMultipleAppInsights
    {
        public StartupMultipleAppInsights(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var botConfig = BotConfiguration.Load("testbot.bot", null);

            services.AddBotApplicationInsights(botConfig, "instance2");

            // Adding IConfiguration in sample test server.  Otherwise this appears to be
            // registered.
            services.AddSingleton(Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            app.UseBotApplicationInsights();
#pragma warning restore CS0618 // Type or member is obsolete
            var telemetryClient = app.ApplicationServices.GetService<IBotTelemetryClient>();
            Assert.NotNull(telemetryClient);
            Assert.Equal(typeof(BotTelemetryClient), telemetryClient.GetType());
        }
    }
}

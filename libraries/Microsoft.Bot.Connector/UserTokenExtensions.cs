// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Bot.Connector
{
    using Microsoft.Bot.Schema;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for UserToken.
    /// </summary>
    public static partial class UserTokenExtensions
    {
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='userId'>
            /// </param>
            /// <param name='connectionName'>
            /// </param>
            /// <param name='channelId'>
            /// </param>
            /// <param name='code'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<TokenResponse> GetTokenAsync(this IUserToken operations, string userId, string connectionName, string channelId = default(string), string code = default(string), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetTokenWithHttpMessagesAsync(userId, connectionName, channelId, code, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='userId'>
            /// </param>
            /// <param name='connectionName'>
            /// </param>
            /// <param name='aadResourceUrls'>
            /// </param>
            /// <param name='channelId'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<IDictionary<string, TokenResponse>> GetAadTokensAsync(this IUserToken operations, string userId, string connectionName, AadResourceUrls aadResourceUrls, string channelId = default(string), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetAadTokensWithHttpMessagesAsync(userId, connectionName, aadResourceUrls, channelId, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='userId'>
            /// </param>
            /// <param name='connectionName'>
            /// </param>
            /// <param name='channelId'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<object> SignOutAsync(this IUserToken operations, string userId, string connectionName = default(string), string channelId = default(string), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.SignOutWithHttpMessagesAsync(userId, connectionName, channelId, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='userId'>
            /// </param>
            /// <param name='channelId'>
            /// </param>
            /// <param name='include'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<IList<TokenStatus>> GetTokenStatusAsync(this IUserToken operations, string userId, string channelId = default(string), string include = default(string), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetTokenStatusWithHttpMessagesAsync(userId, channelId, include, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

    }
}

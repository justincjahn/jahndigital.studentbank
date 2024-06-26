﻿using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;

namespace JahnDigital.StudentBank.Infrastructure.Persistence;

public class SqlAppAuthenticationProvider : SqlAuthenticationProvider
{
    private static readonly AzureServiceTokenProvider _tokenProvider = new();

    /// <summary>
    /// Acquires an access token for SQL using AzureServiceTokenProvider with the given SQL authentication parameters.
    /// </summary>
    /// <param name="parameters">The parameters needed in order to obtain a SQL access token</param>
    /// <returns></returns>
    public override async Task<SqlAuthenticationToken> AcquireTokenAsync(SqlAuthenticationParameters parameters)
    {
        AppAuthenticationResult? authResult = await _tokenProvider
            .GetAuthenticationResultAsync("https://database.windows.net/").ConfigureAwait(false);

        return new SqlAuthenticationToken(authResult.AccessToken, authResult.ExpiresOn);
    }

    /// <summary>
    /// Implements virtual method in SqlAuthenticationProvider. Only Active Directory Interactive Authentication is supported.
    /// </summary>
    /// <param name="authenticationMethod">The SQL authentication method to check whether supported</param>
    /// <returns></returns>
    public override bool IsSupported(SqlAuthenticationMethod authenticationMethod)
    {
        return authenticationMethod == SqlAuthenticationMethod.ActiveDirectoryInteractive;
    }
}

using System;

namespace jahndigital.studentbank.server
{
    /// <summary>
    ///     Configuration settings for the application.
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        ///     Backing field for <see cref="InviteCodeLength" />
        /// </summary>
        private int _inviteCodeLength = 6;

        /// <summary>
        ///     Backing field for <see cref="TokenLifetime" />
        /// </summary>
        private int _tokenLifetime = 60;

        /// <summary>
        ///     AppConfig__Secret is used as a unique secret per-environment for JWT tokens
        ///     and the like.
        /// </summary>
        /// <remarks>
        ///     This should be kept a secret!  A good way to generate this code is to go to a GUID
        ///     generator and slam two of them together.  Instead of placing the secret in a file,
        ///     consider using OS Environment Variables.
        /// </remarks>
        public string Secret { get; set; } = default!;

        /// <summary>
        ///     AppConfig__InviteCodeLength represents the length of invite codes for each instance.
        /// </summary>
        /// <remarks>The default of 6 is probably fine, unless you plan on having a ton of instances.</remarks>
        public int InviteCodeLength
        {
            get => _inviteCodeLength;

            set
            {
                if (value > 38 || value < 3)
                {
                    throw new ArgumentOutOfRangeException(
                        "AppConfig__InviteCodeLength",
                        "Invite codes cannot be longer than 38 charaters or less than 3 characters."
                    );
                }

                _inviteCodeLength = value;
            }
        }

        /// <summary>
        ///     AppConfig__TokenLifetime is the length of time, in minutes, that JWT tokens are valid.
        /// </summary>
        public int TokenLifetime
        {
            get => _tokenLifetime;

            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(
                        "AppConfig__TokenLifetime",
                        "Token lifetimes must be at least 1 minute."
                    );
                }

                _tokenLifetime = value;
            }
        }
    }
}

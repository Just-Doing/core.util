using System;
using System.Collections.Generic;
using System.Text;

namespace Util.Test
{
    public class PowerBIAccessTokenModels
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
        public string expires_on { get; set; }

        public DateTime GetTokenTime { get; set; }
        public string scope { get; set; }
        public int expires_in { get; set; }
        public int ext_expires_in { get; set; }
        public string not_before { get; set; }
        public string resource { get; set; }
        public string refresh_token { get; set; }

        public string AccessTokenType { get; set; }
        public string AccessToken { get; set; }
        public DateTime ExpiresOn { get; set; }

        public string ExtendedExpiresOn { get; set; }
        public string ExtendedLifeTimeToken { get; set; }
        public string TenantId { get; set; }
        public string IdToken { get; set; }

        public string AccessTokenType2 { get { return string.IsNullOrEmpty(token_type) ? AccessTokenType : token_type; } }
        public string AccessToken2 { get { return string.IsNullOrEmpty(access_token) ? AccessToken : access_token; } }
        public DateTime ExpiresOn2
        {
            get
            {
                if (string.IsNullOrEmpty(expires_on))
                {
                    return ExpiresOn;
                }
                else
                {
                    DateTime converted = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    DateTime newDateTime = converted.AddSeconds(double.Parse(expires_on));
                    return newDateTime.ToLocalTime();
                }
            }
        }
    }
}

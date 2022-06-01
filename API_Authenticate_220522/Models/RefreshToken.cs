using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Authenticate_220522.Models
{
    public class RefreshToken
    {
        public string user_name { get; set; }
        public string jwt_token { get; set; }
        public string refresh_token { get; set; }
    }
}

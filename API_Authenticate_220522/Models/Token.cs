using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Authenticate_220522.Models
{
    public class Token
    {
        public string token { get; set; }
        public DateTime expires_at { get; set; }
        public string user_name { get; set; }
        public bool is_expired { get; set; } = false;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Configuration
{
    public class JWTConfig
    {
        public string Secret { get; set; }
        public short TokenLifeTime { get; set; }
        public short RefreshTokenLength { get; set; }
        public short RefreshTokenLifeTime { get; set; }
    }
}

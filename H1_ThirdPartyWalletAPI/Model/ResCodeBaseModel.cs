using H1_ThirdPartyWalletAPI.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model
{
    public class ResCodeBaseModel<T>: ResCodeBase
    {
        public ResCodeBaseModel()
        {
        }

        public T Data { get; set; }

        
    }
}

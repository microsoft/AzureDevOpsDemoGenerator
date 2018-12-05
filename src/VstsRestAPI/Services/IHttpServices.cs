using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Services
{
    public interface IHttpServices
    {
        dynamic Get();
        dynamic Post();
        dynamic Put();
        dynamic Patch();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VstsRestAPI;

namespace VstsDemoBuilder.ServiceInterfaces
{
    public interface IProjectService
    {
        string ReadFromConfiguration(string key);
        Configuration NewConfiguration(string Pat, string accountName, string projectName, string host, string version);
    }
}

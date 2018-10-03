using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VstsRestAPI.Viewmodel.Extractor;

namespace VstsRestAPI.Extractor
{
    public class GetClassificationNodes : ApiServiceBase
    {
        public GetClassificationNodes(IConfiguration configuration) : base(configuration) { }
        
        public IterationtoSave.Nodes GetIterationsToSave(string projectName, string pat, string URL)//string projectName, string URL, string _credentials, string srcProject)
        {
            try
            {

                IterationtoSave.Nodes viewModel = new IterationtoSave.Nodes();
                List<IterationtoSave.Nodes> viewModelList = new List<IterationtoSave.Nodes>();

                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(string.Format("{0}/_apis/wit/classificationNodes/iterations?$depth=1&api-version=1.0", projectName)).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        viewModel = JsonConvert.DeserializeObject<IterationtoSave.Nodes>(result);
                        viewModelList.Add(viewModel);
                        return viewModel;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        LastFailureMessage = error;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return new IterationtoSave.Nodes();
        }
        public GetINumIteration.Iterations GetiterationCount()
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync("/" + Project + "/_apis/work/teamsettings/iterations?api-version=4.1").Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        GetINumIteration.Iterations getINum = JsonConvert.DeserializeObject<GetINumIteration.Iterations>(res);
                        return getINum;
                    }
                    else
                    {
                        string errorMessage = response.Content.ReadAsStringAsync().Result;
                        LastFailureMessage = errorMessage;
                        return new GetINumIteration.Iterations();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return new GetINumIteration.Iterations();
        }
        public ItearationList.Iterations GetIterations()
        {
            try
            {
                ItearationList.Iterations viewModel = new ItearationList.Iterations();
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(string.Format("{0}/_apis/wit/classificationNodes/iterations?$depth=5&api-version=1.0", Project)).Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        viewModel = JsonConvert.DeserializeObject<ItearationList.Iterations>(result);
                        return viewModel;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return new ItearationList.Iterations();
        }

        public SrcTeamsList GetTeamList()
        {
            Teams.TeamList teamObj = new Teams.TeamList();
            SrcTeamsList _team = new SrcTeamsList();
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync("_apis/projects/" + Project + "/teams?api-version=2.2").Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        teamObj = JsonConvert.DeserializeObject<Teams.TeamList>(res);
                        _team = JsonConvert.DeserializeObject<SrcTeamsList>(res);
                        return _team;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        LastFailureMessage = error;
                    }
                }
            }
            catch(Exception ex)
            {
                LastFailureMessage = ex.Message;
            }
            return new SrcTeamsList();
        }
    }
}

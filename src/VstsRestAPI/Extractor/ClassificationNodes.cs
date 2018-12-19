using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using VstsRestAPI.Viewmodel.Extractor;

namespace VstsRestAPI.Extractor
{
    public class ClassificationNodes : ApiServiceBase
    {
        public ClassificationNodes(IConfiguration configuration) : base(configuration) { }

        // Get Iteration Count
        public GetINumIteration.Iterations GetiterationCount()
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(_configuration.UriString + _configuration.Project + "/_apis/work/teamsettings/iterations?api-version=" + _configuration.VersionNumber).Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        GetINumIteration.Iterations getINum = JsonConvert.DeserializeObject<GetINumIteration.Iterations>(result);
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
            catch (Exception)
            {

            }
            return new GetINumIteration.Iterations();
        }
        // Get Iterations to write file
        public ItearationList.Iterations GetIterations()
        {
            try
            {
                ItearationList.Iterations viewModel = new ItearationList.Iterations();
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(string.Format(_configuration.UriString + "/{0}/_apis/wit/classificationNodes/iterations?$depth=5&api-version=" + _configuration.VersionNumber, Project)).Result;
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
                        LastFailureMessage = error;
                    }
                }
            }
            catch (Exception)
            {
            }
            return new ItearationList.Iterations();
        }
        // Get Team List to write to file
        public TeamList ExportTeamList()
        {
            TeamList teamObj = new TeamList();
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(_configuration.UriString + "/_apis/projects/" + Project + "/teams?api-version=" + _configuration.VersionNumber).Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        teamObj = JsonConvert.DeserializeObject<TeamList>(result);
                        return teamObj;
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
                LastFailureMessage = ex.Message;
            }
            return new TeamList();
        }
      
        /// <summary>
        /// GET https://dev.azure.com/fabrikam/Fabrikam/Fabrikam Team/_apis/work/boards/{board}/columns?api-version=4.1
        /// </summary>
        public HttpResponseMessage ExportBoardColums(string boardType)
        {
            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync(string.Format("{0}/{1}/{2}/_apis/work/boards/{3}/columns?api-version={4}", _configuration.UriString, _configuration.Project, _configuration.Team, boardType, _configuration.VersionNumber)).Result;
                return response;
            }
        }

        // Export Board Rows to write file
        //GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards/{board}/rows?api-version=4.1
        public ExportBoardRows.Rows ExportBoardRows(string boardType)
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(string.Format("{0}/{1}/{2}/_apis/work/boards/{3}/rows?api-version={4}", _configuration.UriString, Project, Team, boardType, _configuration.VersionNumber)).Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        ExportBoardRows.Rows rows = new ExportBoardRows.Rows();
                        rows = JsonConvert.DeserializeObject<ExportBoardRows.Rows>(result);
                        return rows;
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
                LastFailureMessage = ex.Message;
            }
            return new ExportBoardRows.Rows();
        }

        // Get Team setting
        //GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/teamsettings?api-version=4.1
        public GetTeamSetting.Setting ExportTeamSetting()
        {
            GetTeamSetting.Setting setting = new GetTeamSetting.Setting();
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(string.Format("{0}/{1}/{2}/_apis/work/teamsettings?api-version={3}", _configuration.UriString, Project, Team, _configuration.VersionNumber)).Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        setting = JsonConvert.DeserializeObject<GetTeamSetting.Setting>(result);
                        return setting;
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
                LastFailureMessage = ex.Message;
            }
            return new GetTeamSetting.Setting();
        }

        //Get Card Fields
        //GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards/{board}/cardsettings?api-version=4.1
        public HttpResponseMessage ExportCardFields(string boardType)
        {
            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync(string.Format("{0}/{1}/{2}/_apis/work/boards/{3}/cardsettings?api-version={4}", _configuration.UriString, Project, Team, boardType, _configuration.VersionNumber)).Result;
                return response;
            }
        }

        //Export Card Styles
        //GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards/{board}/cardrulesettings?api-version=4.1
        public HttpResponseMessage ExportCardStyle(string boardType)
        {
            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync(string.Format("{0}/{1}/{2}/_apis/work/boards/{3}/cardrulesettings?api-version={4}",_configuration.UriString,Project,Team, boardType,_configuration.VersionNumber)).Result;
                return response;
            }
        }
    }
}

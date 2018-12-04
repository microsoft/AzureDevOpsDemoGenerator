using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using VstsRestAPI.Viewmodel.Extractor;

namespace VstsRestAPI.Extractor
{
    public class GetClassificationNodes : ApiServiceBase
    {
        public GetClassificationNodes(IConfiguration configuration) : base(configuration) { }

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
        public SrcTeamsList GetTeamList()
        {
            ListTeams.TeamList teamObj = new ListTeams.TeamList();
            SrcTeamsList _team = new SrcTeamsList();
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(_configuration.UriString + "/_apis/projects/" + Project + "/teams?api-version=" + _configuration.VersionNumber).Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        teamObj = JsonConvert.DeserializeObject<ListTeams.TeamList>(result);

                        _team = JsonConvert.DeserializeObject<SrcTeamsList>(result);
                        for (var x = 0; x < _team.value.Count; x++)
                        {
                            if (_team.value[x].description.ToLower() == "the default project team.")
                            {
                                _team.value.RemoveAt(x);
                            }
                        }

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
            catch (Exception ex)
            {
                LastFailureMessage = ex.Message;
            }
            return new SrcTeamsList();
        }

        // Get Board colums for Scrum template to write to file
        public BoardColumnResponseScrum.ColumnResponse ExportBoardColumnsScrum()
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(_configuration.UriString + "/" + Project + "/" + Project + "%20Team/_apis/work/boards/Backlog%20items/columns?api-version=" + _configuration.VersionNumber).Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        BoardColumnResponseScrum.ColumnResponse columns = Newtonsoft.Json.JsonConvert.DeserializeObject<BoardColumnResponseScrum.ColumnResponse>(response.Content.ReadAsStringAsync().Result.ToString());
                        return columns;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        LastFailureMessage = error;
                        return new BoardColumnResponseScrum.ColumnResponse();
                    }
                }
            }
            catch (Exception ex)
            {
                LastFailureMessage = ex.Message;
            }
            return new BoardColumnResponseScrum.ColumnResponse();
        }

        // Get Board Columns for Agile template to write file
        public BoardColumnResponseAgile.ColumnResponse ExportBoardColumnsAgile()
        {
            try
            {
                BoardColumnResponseAgile.ColumnResponse columns = new BoardColumnResponseAgile.ColumnResponse();
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync("/" + Project + "/" + Project + "%20Team/_apis/work/boards/Stories/columns?api-version=" + _configuration.VersionNumber).Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        columns = JsonConvert.DeserializeObject<BoardColumnResponseAgile.ColumnResponse>(response.Content.ReadAsStringAsync().Result.ToString());
                        return columns;
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
            return new BoardColumnResponseAgile.ColumnResponse();
        }

        // Get Board Rows to write file
        public ExportBoardRows.Rows ExportboardRows()
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(_configuration.UriString + "/" + Project + "/_apis/work/boardrows?api-version=" + _configuration.VersionNumber).Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        ExportBoardRows.Rows rows = new ExportBoardRows.Rows();
                        rows = JsonConvert.DeserializeObject<ExportBoardRows.Rows>(result);
                        ExportBoardRows.Value addValue = new ExportBoardRows.Value();
                        addValue.id = "00000000-0000-0000-0000-000000000000";
                        addValue.name = null;
                        rows.value.Add(addValue);
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

        // Get Card Style details to write file
        public CardStyle.Style GetCardStyle(string boardType)
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(_configuration.UriString + "/" + Project + "/" + Project + "%20team/_apis/work/boards/" + boardType + "/cardrulesettings?api-version=" + _configuration.VersionNumber).Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        CardStyle.Style style = new CardStyle.Style();
                        string result = response.Content.ReadAsStringAsync().Result;
                        style = JsonConvert.DeserializeObject<CardStyle.Style>(result);
                        return style;
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
            return new CardStyle.Style();
        }

        // Get Card fields for Scrum process template to write file
        public CardFiledsScrum.CardField GetCardFieldsScrum()
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(_configuration.UriString + "/" + Project + "/_apis/work/boards/backlog%20items/cardsettings?api-version=" + _configuration.VersionNumber).Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        CardFiledsScrum.CardField card = new CardFiledsScrum.CardField();
                        string result = response.Content.ReadAsStringAsync().Result;
                        card = JsonConvert.DeserializeObject<CardFiledsScrum.CardField>(result);
                        return card;
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
            return new CardFiledsScrum.CardField();
        }

        // Get Card fields for Agile process template to write file
        public CardFiledsAgile.CardField GetCardFieldsAgile()
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync("/" + Project + "/_apis/work/boards/stories/cardsettings?api-version=" + _configuration.VersionNumber).Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        CardFiledsAgile.CardField card = new CardFiledsAgile.CardField();
                        string result = response.Content.ReadAsStringAsync().Result;
                        card = JsonConvert.DeserializeObject<CardFiledsAgile.CardField>(result);
                        return card;
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
            return new CardFiledsAgile.CardField();
        }

        // Get Team setting
        public GetTeamSetting.Setting GetTeamSetting()
        {
            GetTeamSetting.Setting setting = new GetTeamSetting.Setting();
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(_configuration.UriString + "/" + Project + "/_apis/work/teamsettings?api-version=" + _configuration.VersionNumber).Result;
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

        public int GetTeamsCount()
        {
            ListTeams.TeamList teamObj = new ListTeams.TeamList();
            SrcTeamsList _team = new SrcTeamsList();
            using (var client = GetHttpClient())
            {
                //https://dev.azure.com/australiaEastaksh/_apis/projects/SmartHotel360/teams?api-version=4.1
                HttpResponseMessage response = client.GetAsync(_configuration.UriString + "_apis/projects/" + _configuration.Project + "/teams?api-version=" + _configuration.VersionNumber).Result;
                if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string res = response.Content.ReadAsStringAsync().Result;
                    teamObj = JsonConvert.DeserializeObject<ListTeams.TeamList>(res);
                    return teamObj.count;
                }
                else
                {
                    string errorMessage = response.Content.ReadAsStringAsync().Result;
                    LastFailureMessage = errorMessage;
                }
            }
            return 0;
        }
    }
}

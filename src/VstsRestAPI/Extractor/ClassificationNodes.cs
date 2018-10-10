using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
            catch (Exception)
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
            catch (Exception)
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
                        LastFailureMessage = error;
                    }
                }
            }
            catch (Exception)
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
            catch (Exception ex)
            {
                LastFailureMessage = ex.Message;
            }
            return new SrcTeamsList();
        }

        public BoardColumnResponseScrum.ColumnResponse ExportBoardColumnsScrum()
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync("/" + Project + "/" + Project + "%20Team/_apis/work/boards/Backlog%20items/columns?api-version=4.1").Result;
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

        public BoardColumnResponseAgile.ColumnResponse ExportBoardColumnsAgile()
        {
            try
            {
                BoardColumnResponseAgile.ColumnResponse columns = new BoardColumnResponseAgile.ColumnResponse();
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync("/" + Project + "/" + Project + "%20Team/_apis/work/boards/Stories/columns?api-version=4.1").Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        columns = Newtonsoft.Json.JsonConvert.DeserializeObject<BoardColumnResponseAgile.ColumnResponse>(response.Content.ReadAsStringAsync().Result.ToString());
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

        public ExportBoardRows.Rows ExportboardRows()
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync("/" + Project + "/_apis/work/boardrows?api-version=4.1").Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        ExportBoardRows.Rows rows = new ExportBoardRows.Rows();
                        rows = JsonConvert.DeserializeObject<ExportBoardRows.Rows>(res);
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

        public CardStyle.Style GetCardStyle(string boardType)
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync("/" + Project + "/" + Project + "%20team/_apis/work/boards/" + boardType + "/cardrulesettings?api-version=4.1").Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        CardStyle.Style style = new CardStyle.Style();
                        string res = response.Content.ReadAsStringAsync().Result;
                        style = JsonConvert.DeserializeObject<CardStyle.Style>(res);
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

        public CardFiledsScrum.CardField GetCardFieldsScrum()
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync("/" + Project + "/_apis/work/boards/backlog%20items/cardsettings?api-version=4.1").Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        CardFiledsScrum.CardField card = new CardFiledsScrum.CardField();
                        string res = response.Content.ReadAsStringAsync().Result;
                        card = JsonConvert.DeserializeObject<CardFiledsScrum.CardField>(res);
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

        public CardFiledsAgile.CardField GetCardFieldsAgile()
        {
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync("/" + Project + "/_apis/work/boards/stories/cardsettings?api-version=4.1").Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        CardFiledsAgile.CardField card = new CardFiledsAgile.CardField();
                        string res = response.Content.ReadAsStringAsync().Result;
                        card = JsonConvert.DeserializeObject<CardFiledsAgile.CardField>(res);
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


        public GetTeamSetting.Setting GetTeamSetting()
        {
            GetTeamSetting.Setting setting = new GetTeamSetting.Setting();
            try
            {
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage response = client.GetAsync("https://dev.azure.com/" + Account + "/" + Project + "/_apis/work/teamsettings?api-version=4.1").Result;
                    if(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        string res = response.Content.ReadAsStringAsync().Result;
                        setting = JsonConvert.DeserializeObject<GetTeamSetting.Setting>(res);
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
    }
}

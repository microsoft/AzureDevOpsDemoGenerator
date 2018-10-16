using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using VstsRestAPI.Viewmodel.WorkItem;

namespace VstsRestAPI.WorkItemAndTracking
{
    public class ImportWorkItems : ApiServiceBase
    {
        public string boardRowFieldName;
        private List<WIMapData> wiData = new List<WIMapData>();
        private List<string> listAssignToUsers = new List<string>();
        private string[] relTypes = { "Microsoft.VSTS.Common.TestedBy-Reverse", "System.LinkTypes.Hierarchy-Forward", "System.LinkTypes.Related", "System.LinkTypes.Dependency-Reverse", "System.LinkTypes.Dependency-Forward" };
        private string attachmentFolder = string.Empty;
        private string repositoryId = string.Empty;
        private string projectId = string.Empty;
        private Dictionary<string, string> pullRequests = new Dictionary<string, string>();

        public ImportWorkItems(IConfiguration configuration, string rowFieldName) : base(configuration)
        {
            boardRowFieldName = rowFieldName;
        }

        /// <summary>
        /// Import Work items form the files
        /// </summary>
        /// <param name="dicWITypes"></param>
        /// <param name="projectName"></param>
        /// <param name="uniqueUser"></param>
        /// <param name="projectSettingsJson"></param>
        /// <param name="attachmentFolderPath"></param>
        /// <param name="repositoryID"></param>
        /// <param name="projectID"></param>
        /// <param name="dictPullRequests"></param>
        /// <param name="userMethod"></param>
        /// <param name="accountUsers"></param>
        /// <returns></returns>
        
        public List<WIMapData> ImportWorkitems(Dictionary<string, string> dicWITypes, string projectName, string uniqueUser, string projectSettingsJson, string attachmentFolderPath, string repositoryID, string projectID, Dictionary<string, string> dictPullRequests, string userMethod, List<string> accountUsers, string selectedTemplate)
        {
            try
            {
                attachmentFolder = attachmentFolderPath;
                repositoryId = repositoryID;
                projectId = projectID;
                pullRequests = dictPullRequests;
                JArray userList = new JArray();

                if (userMethod == "Select")
                {
                    foreach (string user in accountUsers)
                    {
                        listAssignToUsers.Add(user);
                    }
                }
                else if (userMethod == "Random")
                {
                    if (accountUsers.Count >= 2)
                    {
                        foreach (string user in accountUsers)
                        {
                            listAssignToUsers.Add(user);
                        }
                        if (listAssignToUsers.Count > 10) { listAssignToUsers.RemoveRange(9, listAssignToUsers.Count - 10); }
                    }
                    else
                    {
                        var jitems = JObject.Parse(projectSettingsJson);
                        userList = jitems["users"].Value<JArray>();

                        if (userList.Count > 0)
                        {
                            listAssignToUsers.Add(uniqueUser);
                        }
                        foreach (var data in userList.Values())
                        {
                            listAssignToUsers.Add(data.ToString());
                        }
                    }
                }
                else
                {
                    var jitems = JObject.Parse(projectSettingsJson);
                    userList = jitems["users"].Value<JArray>();

                    if (userList.Count > 0)
                    {
                        listAssignToUsers.Add(uniqueUser);
                    }
                    foreach (var data in userList.Values())
                    {
                        listAssignToUsers.Add(data.ToString());
                    }
                }

                foreach (string wiType in dicWITypes.Keys)
                {
                    PrepareAndUpdateTarget(wiType, dicWITypes[wiType], projectName, selectedTemplate);
                }

                foreach (string wiType in dicWITypes.Keys)
                {
                    UpdateWorkItemLinks(dicWITypes[wiType]);
                }

                return wiData;
            }
            catch (Exception)
            {
                return wiData;
            }

        }

        /// <summary>
        /// Update the work items in Target with all required field values
        /// </summary>
        /// <param name="workItemType"></param>
        /// <param name="workImport"></param>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public bool PrepareAndUpdateTarget(string workItemType, string workImport, string projectName, string selectedTemplate)
        {
            //List<ColumnPost> Columns = JsonConvert.DeserializeObject<List<ColumnPost>>(workImport);
            workImport = workImport.Replace("$ProjectName$", projectName);
            ImportWorkItemModel.WorkItems fetchedWIs = JsonConvert.DeserializeObject<ImportWorkItemModel.WorkItems>(workImport);

            if (fetchedWIs.count > 0)
            {
                foreach (ImportWorkItemModel.Value newWI in fetchedWIs.value)
                {
                    newWI.fields.SystemCreatedDate = DateTime.Now.AddDays(-3);
                    //newWI.fields.SystemChangedDate = DateTime.Now.AddDays(-2);
                    //newWI.fields.SystemRevisedDate = DateTime.Now.AddDays(-2);
                    // UpdateWorkIteminTarget(workItemType,newWI, new String[] {"/fields/System.Title", "/fields/Microsoft.VSTS.CommonPriority","/fields/Microsoft.VSTS.CommonStackRank", "/fields/System.Description"}, new Object[] {newWI.fields.SystemTitle,newWI.fields.MicrosoftVSTSCommonPriority,newWI.fields.MicrosoftVSTSCommonStackRank,newWI.fields.SystemDescription });

                    //String[] FieldstoAdd = { }; Object[] ValuestoAdd = { };
                    Dictionary<string, object> dicWIFields = new Dictionary<string, object>();
                    string assignToUser = string.Empty;
                    if (listAssignToUsers.Count > 0)
                    {
                        assignToUser = listAssignToUsers[new Random().Next(0, listAssignToUsers.Count)];
                    }

                    //Test cases have different fields compared to other items like bug, Epics, etc.                     
                    if ((workItemType == "Test Case"))
                    {
                        //replacing null values with Empty strngs; creation fails if the fields are null
                        if (newWI.fields.MicrosoftVSTSTCMParameters == null)
                        {
                            newWI.fields.MicrosoftVSTSTCMParameters = string.Empty;
                        }

                        if (newWI.fields.MicrosoftVSTSTCMSteps == null)
                        {
                            newWI.fields.MicrosoftVSTSTCMSteps = string.Empty;
                        }

                        if (newWI.fields.MicrosoftVSTSTCMLocalDataSource == null)
                        {
                            newWI.fields.MicrosoftVSTSTCMLocalDataSource = string.Empty;
                        }

                        dicWIFields.Add("/fields/System.Title", newWI.fields.SystemTitle);
                        dicWIFields.Add("/fields/System.State", newWI.fields.SystemState);
                        dicWIFields.Add("/fields/System.Reason", newWI.fields.SystemReason);
                        dicWIFields.Add("/fields/Microsoft.VSTS.Common.Priority", newWI.fields.MicrosoftVSTSCommonPriority);
                        dicWIFields.Add("/fields/Microsoft.VSTS.TCM.Steps", newWI.fields.MicrosoftVSTSTCMSteps);
                        dicWIFields.Add("/fields/Microsoft.VSTS.TCM.Parameters", newWI.fields.MicrosoftVSTSTCMParameters);
                        dicWIFields.Add("/fields/Microsoft.VSTS.TCM.LocalDataSource", newWI.fields.MicrosoftVSTSTCMLocalDataSource);
                        dicWIFields.Add("/fields/Microsoft.VSTS.TCM.AutomationStatus", newWI.fields.MicrosoftVSTSTCMAutomationStatus);

                        if (newWI.fields.MicrosoftVSTSCommonAcceptanceCriteria != null)
                        {
                            dicWIFields.Add("/fields/Microsoft.VSTS.Common.AcceptanceCriteria", newWI.fields.MicrosoftVSTSCommonAcceptanceCriteria);
                        }

                        if (newWI.fields.SystemTags != null)
                        {
                            dicWIFields.Add("/fields/System.Tags", newWI.fields.SystemTags);
                        }

                        dicWIFields.Add("/fields/Microsoft.VSTS.Scheduling.RemainingWork", newWI.fields.MicrosoftVSTSSchedulingRemainingWork);

                    }
                    else
                    {
                        string iterationPath = projectName;
                        string boardRowField = string.Empty;

                        if (newWI.fields.SystemIterationPath.Contains("\\"))
                        {
                            iterationPath = string.Format(@"{0}\{1}", projectName, newWI.fields.SystemIterationPath.Split('\\')[1]);

                        }

                        if (!string.IsNullOrWhiteSpace(boardRowFieldName))
                        {
                            boardRowField = string.Format("/fields/{0}", boardRowFieldName);
                        }

                        if (newWI.fields.SystemDescription == null)
                        {
                            newWI.fields.SystemDescription = newWI.fields.SystemTitle;
                        }

                        if (string.IsNullOrEmpty(newWI.fields.SystemBoardLane))
                        {
                            newWI.fields.SystemBoardLane = string.Empty;
                        }

                        dicWIFields.Add("/fields/System.Title", newWI.fields.SystemTitle);
                        if (selectedTemplate.ToLower() == "smarthotel360")
                        {
                            dicWIFields.Add("/fields/System.AreaPath", newWI.fields.SystemAreaPath);
                        }
                        dicWIFields.Add("/fields/System.Description", newWI.fields.SystemDescription);
                        dicWIFields.Add("/fields/System.State", newWI.fields.SystemState);
                        dicWIFields.Add("/fields/System.Reason", newWI.fields.SystemReason);
                        dicWIFields.Add("/fields/Microsoft.VSTS.Common.Priority", newWI.fields.MicrosoftVSTSCommonPriority);
                        dicWIFields.Add("/fields/System.AssignedTo", assignToUser);
                        dicWIFields.Add("/fields/System.IterationPath", iterationPath);
                        dicWIFields.Add("/fields/Microsoft.VSTS.Scheduling.RemainingWork", newWI.fields.MicrosoftVSTSSchedulingRemainingWork);
                        dicWIFields.Add("/fields/Microsoft.VSTS.Scheduling.Effort", newWI.fields.MicrosoftVSTSSchedulingEffort);

                        if (newWI.fields.MicrosoftVSTSCommonAcceptanceCriteria != null)
                        {
                            dicWIFields.Add("/fields/Microsoft.VSTS.Common.AcceptanceCriteria", newWI.fields.MicrosoftVSTSCommonAcceptanceCriteria);
                        }

                        if (newWI.fields.SystemTags != null)
                        {
                            dicWIFields.Add("/fields/System.Tags", newWI.fields.SystemTags);
                        }

                        if (newWI.fields.MicrosoftVSTSTCMParameters != null)
                        {
                            dicWIFields.Add("/fields/Microsoft.VSTS.TCM.Parameters", newWI.fields.MicrosoftVSTSTCMParameters);
                        }

                        if (newWI.fields.MicrosoftVSTSTCMSteps != null)
                        {
                            dicWIFields.Add("/fields/Microsoft.VSTS.TCM.Steps", newWI.fields.MicrosoftVSTSTCMSteps);
                        }

                        if (!string.IsNullOrWhiteSpace(boardRowField))
                        {
                            dicWIFields.Add(boardRowField, newWI.fields.SystemBoardLane);
                        }
                    }
                    UpdateWorkIteminTarget(workItemType, newWI.id.ToString(), projectName, dicWIFields);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Update work ietm with all required field values
        /// </summary>
        /// <param name="workItemType"></param>
        /// <param name="old_wi_ID"></param>
        /// <param name="projectName"></param>
        /// <param name="dictionaryWIFields"></param>
        /// <returns></returns>
        public bool UpdateWorkIteminTarget(string workItemType, string old_wi_ID, string projectName, Dictionary<string, object> dictionaryWIFields)
        {
            //int pathCount = paths.Count();
            List<WorkItemPatch.Field> listFields = new List<WorkItemPatch.Field>();
            WorkItemPatchResponse.WorkItem viewModel = new WorkItemPatchResponse.WorkItem();
            // change some values on a few fields
            foreach (string key in dictionaryWIFields.Keys)
            {
                listFields.Add(new WorkItemPatch.Field() { op = "add", path = key, value = dictionaryWIFields[key] });
            }
            WorkItemPatch.Field[] fields = listFields.ToArray();
            using (var client = GetHttpClient())
            {
                //var postValue = new StringContent(JsonConvert.SerializeObject(wI), Encoding.UTF8, "application/json"); // mediaType needs to be application/json-patch+json for a patch call
                var postValue = new StringContent(JsonConvert.SerializeObject(fields), Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call
                // set the httpmethod to Patch
                var method = new HttpMethod("PATCH");

                // send the request               
                var request = new HttpRequestMessage(method, projectName + "/_apis/wit/workitems/$" + workItemType + "?bypassRules=true&api-version=" + _configuration.VersionNumber) { Content = postValue };
                var response = client.SendAsync(request).Result;


                if (response.IsSuccessStatusCode)
                {
                    viewModel = response.Content.ReadAsAsync<WorkItemPatchResponse.WorkItem>().Result;
                    wiData.Add(new WIMapData() { OldID = old_wi_ID, NewID = viewModel.id.ToString(), WIType = workItemType });
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                    this.LastFailureMessage = error;
                }

                return response.IsSuccessStatusCode;

            }
        }
        /// <summary>
        /// Update work items links - parent child- Hyperlinks-artifact links-attachments
        /// </summary>
        /// <param name="workItemTemplateJson"></param>
        /// <returns></returns>
        public bool UpdateWorkItemLinks(string workItemTemplateJson)
        {
            ImportWorkItemModel.WorkItems fetchedPBIs = JsonConvert.DeserializeObject<ImportWorkItemModel.WorkItems>(workItemTemplateJson);
            //ImportWorkItemModel.WorkItems fetchedPBIs
            string wiToUpdate = "";
            WIMapData findIDforUpdate;
            if (fetchedPBIs.count > 0)
            {

                foreach (ImportWorkItemModel.Value newWI in fetchedPBIs.value)
                {
                    //continue next iteration if there is no relation
                    if (newWI.relations == null)
                    {
                        continue;
                    }

                    int relCount = newWI.relations.Length;
                    string oldWIID = newWI.id.ToString();

                    findIDforUpdate = wiData.Find(t => t.OldID == oldWIID);
                    if (findIDforUpdate != null)
                    {
                        wiToUpdate = findIDforUpdate.NewID;
                        foreach (ImportWorkItemModel.Relations rel in newWI.relations)
                        {
                            if (relTypes.Contains(rel.rel.Trim()))
                            {
                                oldWIID = rel.url.Substring(rel.url.LastIndexOf("/") + 1);
                                WIMapData findIDforlink = wiData.Find(t => t.OldID == oldWIID);

                                if (findIDforlink != null)
                                {
                                    string newWIID = findIDforlink.NewID;
                                    Object[] patchWorkItem = new Object[1];
                                    // change some values on a few fields
                                    patchWorkItem[0] = new
                                    {
                                        op = "add",
                                        path = "/relations/-",
                                        value = new
                                        {
                                            rel = rel.rel,
                                            url = _configuration.UriString + "/_apis/wit/workitems/" + newWIID,
                                            attributes = new
                                            {
                                                comment = "Making a new link for the dependency"
                                            }
                                        }
                                    };
                                    //UpdateWorkIteminTarget("Product Backlog Item", newWI.id.ToString(), new String[] { "/relations/-"}, new Object[] { newWI.fields.SystemTitle, newWI.fields.SystemDescription });
                                    if (UpdateLink("Product Backlog Item", wiToUpdate, patchWorkItem))
                                    {
                                        //Console.WriteLine("Updated WI with link from {0} to {1}", oldWIID, newWIID);
                                    }
                                }
                            }
                            if (rel.rel == "Hyperlink")
                            {
                                Object[] patchWorkItem = new Object[1];
                                patchWorkItem[0] = new
                                {
                                    op = "add",
                                    path = "/relations/-",
                                    value = new
                                    {
                                        rel = "Hyperlink",
                                        url = rel.url
                                    }
                                };
                                bool isHyperLinkCreated = UpdateLink(string.Empty, wiToUpdate, patchWorkItem);
                            }
                            if (rel.rel == "AttachedFile")
                            {
                                Object[] patchWorkItem = new Object[1];
                                string filPath = string.Format(attachmentFolder + @"\{0}{1}", rel.attributes["id"], rel.attributes["name"]);
                                string fileName = rel.attributes["name"];
                                string attchmentURl = UploadAttchment(filPath, fileName);
                                if (!string.IsNullOrEmpty(attchmentURl))
                                {
                                    patchWorkItem[0] = new
                                    {
                                        op = "add",
                                        path = "/relations/-",
                                        value = new
                                        {
                                            rel = "AttachedFile",
                                            url = attchmentURl
                                        }
                                    };
                                    bool isAttachmemntCreated = UpdateLink(string.Empty, wiToUpdate, patchWorkItem);
                                }
                            }
                            if (rel.rel == "ArtifactLink")
                            {
                                rel.url = rel.url.Replace("$projectId$", projectId).Replace("$RepositoryId$", repositoryId);
                                foreach (var pullReqest in pullRequests)
                                {
                                    string key = string.Format("${0}$", pullReqest.Key);
                                    rel.url = rel.url.Replace(key, pullReqest.Value);
                                }
                                Object[] patchWorkItem = new Object[1];
                                patchWorkItem[0] = new
                                {
                                    op = "add",
                                    path = "/relations/-",
                                    value = new
                                    {
                                        rel = "ArtifactLink",
                                        url = rel.url,
                                        attributes = new
                                        {
                                            name = rel.attributes["name"]
                                        }
                                    }

                                };
                                bool isArtifactLinkCreated = UpdateLink(string.Empty, wiToUpdate, patchWorkItem);
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Udpate Links to work items
        /// </summary>
        /// <param name="workItemType"></param>
        /// <param name="witoUpdate"></param>
        /// <param name="patchWorkItem"></param>
        /// <returns></returns>
        public bool UpdateLink(string workItemType, string witoUpdate, object[] patchWorkItem)
        {
            using (var client = GetHttpClient())
            {
                // serialize the fields array into a json string          
                var patchValue = new StringContent(JsonConvert.SerializeObject(patchWorkItem), Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call

                var method = new HttpMethod("PATCH");
                var request = new HttpRequestMessage(method, Project + "/_apis/wit/workitems/" + witoUpdate + "?bypassRules=true&api-version=" + _configuration.VersionNumber) { Content = patchValue };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    // viewModel = response.Content.ReadAsAsync<WorkItemPatchResponse.WorkItem>().Result;
                }
                //viewModel.HttpStatusCode = response.StatusCode;

                return response.IsSuccessStatusCode;
            }
        }
        /// <summary>
        /// Upload attachments to VSTS server
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string UploadAttchment(string filePath, string fileName)
        {
            try
            {
                string _filePath = filePath;
                string _fileName = fileName;

                if (File.Exists(filePath))
                {
                    //read file bytes and put into byte array        
                    Byte[] bytes = File.ReadAllBytes(filePath);

                    using (var client = GetHttpClient())
                    {
                        ByteArrayContent content = new ByteArrayContent(bytes);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        HttpResponseMessage uploadResponse = client.PostAsync("_apis/wit/attachments?fileName=" + _fileName + "&api-version=" + _configuration.VersionNumber, content).Result;

                        if (uploadResponse.IsSuccessStatusCode)
                        {
                            //get the result, we need this to get the url of the attachment
                            string attachmentURL = JObject.Parse(uploadResponse.Content.ReadAsStringAsync().Result)["url"].ToString();
                            return attachmentURL;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return string.Empty;
        }
    }

    public class WIMapData
    {
        public string OldID { get; set; }
        public string NewID { get; set; }
        public string WIType { get; set; }
    }
}

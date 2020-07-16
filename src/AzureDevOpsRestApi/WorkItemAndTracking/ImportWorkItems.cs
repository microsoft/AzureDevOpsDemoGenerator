using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using AzureDevOpsAPI.Viewmodel.WorkItem;

namespace AzureDevOpsAPI.WorkItemAndTracking
{
    public class ImportWorkItems : ApiServiceBase
    {
        public string boardRowFieldName;
        private List<WiMapData> wiData = new List<WiMapData>();
        private List<string> listAssignToUsers = new List<string>();
        private string[] relTypes = { "Microsoft.VSTS.Common.TestedBy-Reverse", "System.LinkTypes.Hierarchy-Forward", "System.LinkTypes.Related", "System.LinkTypes.Dependency-Reverse", "System.LinkTypes.Dependency-Forward" };
        private string attachmentFolder = string.Empty;
        private string repositoryId = string.Empty;
        private string projectId = string.Empty;
        private Dictionary<string, string> pullRequests = new Dictionary<string, string>();
        Logger logger = LogManager.GetLogger("*");
        public ImportWorkItems(IAppConfiguration configuration, string rowFieldName) : base(configuration)
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

        public List<WiMapData> ImportWorkitems(Dictionary<string, string> dicWITypes, string projectName, string uniqueUser, string projectSettingsJson, string attachmentFolderPath, string repositoryID, string projectID, Dictionary<string, string> dictPullRequests, string userMethod, List<string> accountUsers, string selectedTemplate)
        {
            try
            {
                attachmentFolder = attachmentFolderPath;
                repositoryId = repositoryID;
                projectId = projectID;
                pullRequests = dictPullRequests;
                JArray userList = new JArray();
                JToken userAssignment = null;
                if (userMethod == "Select")
                {
                    foreach (string user in accountUsers)
                    {
                        listAssignToUsers.Add(user);
                    }
                }
                else
                {
                    var jitems = JObject.Parse(projectSettingsJson);
                    userList = jitems["users"].Value<JArray>();
                    userAssignment = jitems["userAssignment"];
                    if (userList.Count > 0)
                    {
                        listAssignToUsers.Add(uniqueUser);
                    }
                    else
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
                    PrepareAndUpdateTarget(wiType, dicWITypes[wiType], projectName, selectedTemplate, userAssignment == null ? "" : userAssignment.ToString());
                }

                foreach (string wiType in dicWITypes.Keys)
                {
                    UpdateWorkItemLinks(dicWITypes[wiType]);
                }

                return wiData;
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
        public bool PrepareAndUpdateTarget(string workItemType, string workImport, string projectName, string selectedTemplate, string userAssignment)
        {
            try
            {
                workImport = workImport.Replace("$ProjectName$", projectName);
                ImportWorkItemModel.WorkItems fetchedWIs = JsonConvert.DeserializeObject<ImportWorkItemModel.WorkItems>(workImport);

                if (fetchedWIs.Count > 0)
                {
                    if (workItemType.ToLower() == "epic" || workItemType.ToLower() == "feature")
                    {
                        //fetchedWIs.Value = fetchedWIs.Value.OrderBy(x => x.Id).ToArray();
                    }
                    foreach (ImportWorkItemModel.Value newWI in fetchedWIs.Value)
                    {
                        newWI.Fields.SystemCreatedDate = DateTime.Now.AddDays(-3);
                        Dictionary<string, object> dicWIFields = new Dictionary<string, object>();
                        string assignToUser = string.Empty;
                        if (listAssignToUsers.Count > 0)
                        {
                            assignToUser = listAssignToUsers[new Random().Next(0, listAssignToUsers.Count)] ?? string.Empty;
                        }

                        //Test cases have different fields compared to other items like bug, Epics, etc.                     
                        if ((workItemType == "Test Case"))
                        {
                            //replacing null values with Empty strngs; creation fails if the fields are null
                            if (newWI.Fields.MicrosoftVststcmParameters == null)
                            {
                                newWI.Fields.MicrosoftVststcmParameters = string.Empty;
                            }

                            if (newWI.Fields.MicrosoftVststcmSteps == null)
                            {
                                newWI.Fields.MicrosoftVststcmSteps = string.Empty;
                            }

                            if (newWI.Fields.MicrosoftVststcmLocalDataSource == null)
                            {
                                newWI.Fields.MicrosoftVststcmLocalDataSource = string.Empty;
                            }

                            dicWIFields.Add("/fields/System.Title", newWI.Fields.SystemTitle);
                            dicWIFields.Add("/fields/System.State", newWI.Fields.SystemState);
                            dicWIFields.Add("/fields/System.Reason", newWI.Fields.SystemReason);
                            dicWIFields.Add("/fields/Microsoft.VSTS.Common.Priority", newWI.Fields.MicrosoftVstsCommonPriority);
                            dicWIFields.Add("/fields/Microsoft.VSTS.TCM.Steps", newWI.Fields.MicrosoftVststcmSteps);
                            dicWIFields.Add("/fields/Microsoft.VSTS.TCM.Parameters", newWI.Fields.MicrosoftVststcmParameters);
                            dicWIFields.Add("/fields/Microsoft.VSTS.TCM.LocalDataSource", newWI.Fields.MicrosoftVststcmLocalDataSource);
                            dicWIFields.Add("/fields/Microsoft.VSTS.TCM.AutomationStatus", newWI.Fields.MicrosoftVststcmAutomationStatus);

                            if (newWI.Fields.MicrosoftVstsCommonAcceptanceCriteria != null)
                            {
                                dicWIFields.Add("/fields/Microsoft.VSTS.Common.AcceptanceCriteria", newWI.Fields.MicrosoftVstsCommonAcceptanceCriteria);
                            }

                            if (newWI.Fields.SystemTags != null)
                            {
                                dicWIFields.Add("/fields/System.Tags", newWI.Fields.SystemTags);
                            }

                            dicWIFields.Add("/fields/Microsoft.VSTS.Scheduling.RemainingWork", newWI.Fields.MicrosoftVstsSchedulingRemainingWork);

                        }
                        else
                        {
                            string iterationPath = projectName;
                            string boardRowField = string.Empty;

                            if (newWI.Fields.SystemIterationPath.Contains("\\"))
                            {
                                iterationPath = string.Format("{0}\\{1}", projectName, newWI.Fields.SystemIterationPath.Split('\\')[1]);
                            }

                            if (!string.IsNullOrWhiteSpace(boardRowFieldName))
                            {
                                boardRowField = string.Format("/fields/{0}", boardRowFieldName);
                            }

                            if (newWI.Fields.SystemDescription == null)
                            {
                                newWI.Fields.SystemDescription = newWI.Fields.SystemTitle;
                            }

                            if (string.IsNullOrEmpty(newWI.Fields.SystemBoardLane))
                            {
                                newWI.Fields.SystemBoardLane = string.Empty;
                            }

                            dicWIFields.Add("/fields/System.Title", newWI.Fields.SystemTitle);
                            if (userAssignment.ToLower() != "any")
                            {
                                if (newWI.Fields.SystemState == "Done")
                                {
                                    dicWIFields.Add("/fields/System.AssignedTo", assignToUser);
                                }
                            }
                            else
                            {
                                dicWIFields.Add("/fields/System.AssignedTo", assignToUser);
                            }
                            string areaPath = newWI.Fields.SystemAreaPath ?? projectName;
                            string[] areaPathSlpit = areaPath.Split('/');
                            areaPathSlpit[0] = projectName;
                            areaPath = string.Join("//", areaPathSlpit);
                            dicWIFields.Add("/fields/System.AreaPath", areaPath);
                            dicWIFields.Add("/fields/System.Description", newWI.Fields.SystemDescription);
                            dicWIFields.Add("/fields/System.State", newWI.Fields.SystemState);
                            dicWIFields.Add("/fields/System.Reason", newWI.Fields.SystemReason);
                            dicWIFields.Add("/fields/Microsoft.VSTS.Common.Priority", newWI.Fields.MicrosoftVstsCommonPriority);
                            dicWIFields.Add("/fields/System.IterationPath", iterationPath);
                            dicWIFields.Add("/fields/Microsoft.VSTS.Scheduling.RemainingWork", newWI.Fields.MicrosoftVstsSchedulingRemainingWork);
                            dicWIFields.Add("/fields/Microsoft.VSTS.Scheduling.Effort", newWI.Fields.MicrosoftVstsSchedulingEffort);

                            if (newWI.Fields.MicrosoftVstsCommonAcceptanceCriteria != null)
                            {
                                dicWIFields.Add("/fields/Microsoft.VSTS.Common.AcceptanceCriteria", newWI.Fields.MicrosoftVstsCommonAcceptanceCriteria);
                            }

                            if (newWI.Fields.SystemTags != null)
                            {
                                dicWIFields.Add("/fields/System.Tags", newWI.Fields.SystemTags);
                            }

                            if (newWI.Fields.MicrosoftVststcmParameters != null)
                            {
                                dicWIFields.Add("/fields/Microsoft.VSTS.TCM.Parameters", newWI.Fields.MicrosoftVststcmParameters);
                            }

                            if (newWI.Fields.MicrosoftVststcmSteps != null)
                            {
                                dicWIFields.Add("/fields/Microsoft.VSTS.TCM.Steps", newWI.Fields.MicrosoftVststcmSteps);
                            }

                            if (!string.IsNullOrWhiteSpace(boardRowField))
                            {
                                dicWIFields.Add(boardRowField, newWI.Fields.SystemBoardLane);
                            }
                        }
                        UpdateWorkIteminTarget(workItemType, newWI.Id.ToString(), projectName, dicWIFields);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
            try
            {
                List<WorkItemPatch.Field> listFields = new List<WorkItemPatch.Field>();
                WorkItemPatchResponse.WorkItem viewModel = new WorkItemPatchResponse.WorkItem();
                // change some values on a few fields
                foreach (string key in dictionaryWIFields.Keys)
                {
                    listFields.Add(new WorkItemPatch.Field() { Op = "add", Path = key, Value = dictionaryWIFields[key] });
                }
                WorkItemPatch.Field[] fields = listFields.ToArray();
                using (var client = GetHttpClient())
                {
                    var postValue = new StringContent(JsonConvert.SerializeObject(fields), Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call
                    var method = new HttpMethod("PATCH");
                    // send the request               
                    var request = new HttpRequestMessage(method, projectName + "/_apis/wit/workitems/$" + workItemType + "?bypassRules=true&api-version=" + Configuration.VersionNumber) { Content = postValue };
                    var response = client.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        viewModel = response.Content.ReadAsAsync<WorkItemPatchResponse.WorkItem>().Result;
                        wiData.Add(new WiMapData() { OldId = old_wi_ID, NewId = viewModel.Id.ToString(), WiType = workItemType });
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        string error = Utility.GeterroMessage(errorMessage.Result.ToString());
                        this.LastFailureMessage = error;
                        logger.Info(error);
                    }

                    return response.IsSuccessStatusCode;
                }
            }
            catch (OperationCanceledException opr)
            {
                logger.Debug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t OperationCanceledException: " + opr.Message + "\t" + "\n" + opr.StackTrace + "\n");
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return false;
        }
        /// <summary>
        /// Update work items links - parent child- Hyperlinks-artifact links-attachments
        /// </summary>
        /// <param name="workItemTemplateJson"></param>
        /// <returns></returns>
        public bool UpdateWorkItemLinks(string workItemTemplateJson)
        {
            try
            {
                ImportWorkItemModel.WorkItems fetchedPBIs = JsonConvert.DeserializeObject<ImportWorkItemModel.WorkItems>(workItemTemplateJson);
                string wiToUpdate = "";
                WiMapData findIDforUpdate;
                if (fetchedPBIs.Count > 0)
                {
                    foreach (ImportWorkItemModel.Value newWI in fetchedPBIs.Value)
                    {
                        //continue next iteration if there is no relation
                        if (newWI.Relations == null)
                        {
                            continue;
                        }

                        int relCount = newWI.Relations.Length;
                        string oldWIID = newWI.Id.ToString();

                        findIDforUpdate = wiData.Find(t => t.OldId == oldWIID);
                        if (findIDforUpdate != null)
                        {
                            wiToUpdate = findIDforUpdate.NewId;
                            foreach (ImportWorkItemModel.Relations rel in newWI.Relations)
                            {
                                if (relTypes.Contains(rel.Rel.Trim()))
                                {
                                    oldWIID = rel.Url.Substring(rel.Url.LastIndexOf("/") + 1);
                                    WiMapData findIDforlink = wiData.Find(t => t.OldId == oldWIID);

                                    if (findIDforlink != null)
                                    {
                                        string newWIID = findIDforlink.NewId;
                                        Object[] patchWorkItem = new Object[1];
                                        // change some values on a few fields
                                        patchWorkItem[0] = new
                                        {
                                            op = "add",
                                            path = "/relations/-",
                                            value = new
                                            {
                                                rel = rel.Rel,
                                                url = Configuration.UriString + "/_apis/wit/workitems/" + newWIID,
                                                attributes = new
                                                {
                                                    comment = "Making a new link for the dependency"
                                                }
                                            }
                                        };
                                        if (UpdateLink("Product Backlog Item", wiToUpdate, patchWorkItem))
                                        {
                                        }
                                    }
                                }
                                if (rel.Rel == "Hyperlink")
                                {
                                    Object[] patchWorkItem = new Object[1];
                                    patchWorkItem[0] = new
                                    {
                                        op = "add",
                                        path = "/relations/-",
                                        value = new
                                        {
                                            rel = "Hyperlink",
                                            url = rel.Url
                                        }
                                    };
                                    bool isHyperLinkCreated = UpdateLink(string.Empty, wiToUpdate, patchWorkItem);
                                }
                                if (rel.Rel == "AttachedFile")
                                {
                                    Object[] patchWorkItem = new Object[1];
                                    string filPath = string.Format(attachmentFolder + @"\{0}{1}", rel.Attributes["id"], rel.Attributes["name"]);
                                    string fileName = rel.Attributes["name"];
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
                                if (rel.Rel == "ArtifactLink")
                                {
                                    rel.Url = rel.Url.Replace("$projectId$", projectId).Replace("$RepositoryId$", repositoryId);
                                    foreach (var pullReqest in pullRequests)
                                    {
                                        string key = string.Format("${0}$", pullReqest.Key);
                                        rel.Url = rel.Url.Replace(key, pullReqest.Value);
                                    }
                                    Object[] patchWorkItem = new Object[1];
                                    patchWorkItem[0] = new
                                    {
                                        op = "add",
                                        path = "/relations/-",
                                        value = new
                                        {
                                            rel = "ArtifactLink",
                                            url = rel.Url,
                                            attributes = new
                                            {
                                                name = rel.Attributes["name"]
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
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
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
            try
            {
                using (var client = GetHttpClient())
                {
                    // serialize the fields array into a json string          
                    var patchValue = new StringContent(JsonConvert.SerializeObject(patchWorkItem), Encoding.UTF8, "application/json-patch+json"); // mediaType needs to be application/json-patch+json for a patch call

                    var method = new HttpMethod("PATCH");
                    var request = new HttpRequestMessage(method, Project + "/_apis/wit/workitems/" + witoUpdate + "?bypassRules=true&api-version=" + Configuration.VersionNumber) { Content = patchValue };
                    var response = client.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                    }

                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return false;
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
                        HttpResponseMessage uploadResponse = client.PostAsync("_apis/wit/attachments?fileName=" + _fileName + "&api-version=" + Configuration.VersionNumber, content).Result;

                        if (uploadResponse.IsSuccessStatusCode)
                        {
                            //get the result, we need this to get the url of the attachment
                            string attachmentURL = JObject.Parse(uploadResponse.Content.ReadAsStringAsync().Result)["url"].ToString();
                            return attachmentURL;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                return string.Empty;
            }
            return string.Empty;
        }
    }

    public class WiMapData
    {
        public string OldId { get; set; }
        public string NewId { get; set; }
        public string WiType { get; set; }
    }
}

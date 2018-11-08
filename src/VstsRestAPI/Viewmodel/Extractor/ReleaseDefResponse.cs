using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Viewmodel.Extractor
{

    public class ReleaseDefResponse
    {
        public class HostingPlan
        {
            public string value { get; set; }
        }

        public class ResourceGroupName
        {
            public string value { get; set; }
        }

        public class ServerName
        {
            public string value { get; set; }
        }

        public class Variables
        {
            public HostingPlan HostingPlan { get; set; }
            public ResourceGroupName ResourceGroupName { get; set; }
            public ServerName ServerName { get; set; }
        }

        public class Owner
        {
            public string id { get; set; }
            public string displayName { get; set; }
            public string uniqueName { get; set; }
        }

        public class PreApproval
        {
            public int rank { get; set; }
            public bool isAutomated { get; set; }
            public bool isNotificationOn { get; set; }
        }

        public class PreDeployApprovals
        {
            public IList<PreApproval> approvals { get; set; }
        }

        public class DeployStep
        {
        }

        public class PostApproval
        {
            public int rank { get; set; }
            public bool isAutomated { get; set; }
            public bool isNotificationOn { get; set; }
        }

        public class PostDeployApprovals
        {
            public IList<PostApproval> approvals { get; set; }
        }

        public class ParallelExecution
        {
            public string parallelExecutionType { get; set; }
        }

        public class ArtifactsDownloadInput
        {
            public IList<object> downloadInputs { get; set; }
        }

        public class OverrideInputs
        {
        }

        public class DeploymentInput
        {
            public ParallelExecution parallelExecution { get; set; }
            public bool skipArtifactsDownload { get; set; }
            public ArtifactsDownloadInput artifactsDownloadInput { get; set; }
            public string queueId { get; set; }
            public IList<object> demands { get; set; }
            public bool enableAccessToken { get; set; }
            public int timeoutInMinutes { get; set; }
            public int jobCancelTimeoutInMinutes { get; set; }
            public string condition { get; set; }
            public OverrideInputs overrideInputs { get; set; }
        }

        public class Inputs
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string ConnectedServiceName { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string ConnectedServiceNameARM { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string action { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string resourceGroupName { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string location { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string templateLocation { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string csmFileLink { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string csmParametersFileLink { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string csmFile { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string csmParametersFile { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string overrideParameters { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string deploymentMode { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string enableDeploymentPrerequisites { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string deploymentGroupEndpoint { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string project { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string deploymentGroupName { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string copyAzureVMTags { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string outputVariable { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string WebAppName { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string WebAppKind { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string DeployToSlotFlag { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string ImageSource { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string ResourceGroupName { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string SlotName { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string AzureContainerRegistry { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string AzureContainerRegistryLoginServer { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string AzureContainerRegistryImage { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string AzureContainerRegistryTag { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string DockerRepositoryAccess { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string RegistryConnectedServiceName { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string PrivateRegistryImage { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string PrivateRegistryTag { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string DockerNamespace { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string DockerRepository { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string DockerImageTag { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string VirtualApplication { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Package { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string BuiltinLinuxPackage { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string RuntimeStack { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string StartupCommand { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string WebAppUri { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string ScriptType { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string InlineScript { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string ScriptPath { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string GenerateWebConfig { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string WebConfigParameters { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string AppSettings { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string TakeAppOfflineFlag { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string UseWebDeploy { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string SetParametersFile { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string RemoveAdditionalFilesFlag { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string ExcludeFilesFromAppDataFlag { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string AdditionalArguments { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string RenameFilesFlag { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string XmlTransformation { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string XmlVariableSubstitution { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string JSONFiles { get; set; }
        }

        public class WorkflowTask
        {
            public string taskId { get; set; }
            public string version { get; set; }
            public string name { get; set; }
            public string refName { get; set; }
            public bool enabled { get; set; }
            public bool alwaysRun { get; set; }
            public bool continueOnError { get; set; }
            public int timeoutInMinutes { get; set; }
            public string definitionType { get; set; }
            public OverrideInputs overrideInputs { get; set; }
            public string condition { get; set; }
            public Inputs inputs { get; set; }
        }

        public class DeployPhas
        {
            public DeploymentInput deploymentInput { get; set; }
            public int rank { get; set; }
            public string phaseType { get; set; }
            public string name { get; set; }
            public IList<WorkflowTask> workflowTasks { get; set; }
        }

        public class EnvironmentOptions
        {
            public string emailNotificationType { get; set; }
            public string emailRecipients { get; set; }
            public bool skipArtifactsDownload { get; set; }
            public int timeoutInMinutes { get; set; }
            public bool enableAccessToken { get; set; }
            public bool publishDeploymentStatus { get; set; }
        }

        public class Condition
        {
            public string name { get; set; }
            public string conditionType { get; set; }
            public string value { get; set; }
        }

        public class ExecutionPolicy
        {
            public int concurrencyCount { get; set; }
            public int queueDepthCount { get; set; }
        }

        public class RetentionPolicy
        {
            public int daysToKeep { get; set; }
            public int releasesToKeep { get; set; }
            public bool retainBuild { get; set; }
        }

        public class ProcessParameters
        {
        }

        public class Properties
        {
        }

        public class PreDeploymentGates
        {
            public object gatesOptions { get; set; }
            public IList<object> gates { get; set; }
        }

        public class PostDeploymentGates
        {
            public object gatesOptions { get; set; }
            public IList<object> gates { get; set; }
        }

        public class Environment
        {
            public string name { get; set; }
            public int rank { get; set; }
            public Owner owner { get; set; }
            public Variables variables { get; set; }
            public IList<object> variableGroups { get; set; }
            public PreDeployApprovals preDeployApprovals { get; set; }
            public DeployStep deployStep { get; set; }
            public PostDeployApprovals postDeployApprovals { get; set; }
            public IList<DeployPhas> deployPhases { get; set; }
            public EnvironmentOptions environmentOptions { get; set; }
            public IList<object> demands { get; set; }
            public IList<Condition> conditions { get; set; }
            public ExecutionPolicy executionPolicy { get; set; }
            public IList<object> schedules { get; set; }
            public RetentionPolicy retentionPolicy { get; set; }
            public ProcessParameters processParameters { get; set; }
            public Properties properties { get; set; }
            public PreDeploymentGates preDeploymentGates { get; set; }
            public PostDeploymentGates postDeploymentGates { get; set; }
        }

        public class DefaultVersionBranch
        {
            public string name { get; set; }
        }

        public class DefaultVersionSpecific
        {
            public string name { get; set; }
        }

        public class DefaultVersionTags
        {
            public string name { get; set; }
        }

        public class DefaultVersionType
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class Definition
        {
            public string id { get; set; }
            public string name { get; set; }
        }
        public class Project
        {
            public string id { get; set; }
            public string name { get; set; }
        }
        public class DefinitionReference
        {
            public DefaultVersionBranch defaultVersionBranch { get; set; }
            public DefaultVersionSpecific defaultVersionSpecific { get; set; }
            public DefaultVersionTags defaultVersionTags { get; set; }
            public DefaultVersionType defaultVersionType { get; set; }
            public Definition definition { get; set; }
            public Project project { get; set; }
        }

        public class Artifact
        {
            public string sourceId { get; set; }
            public string type { get; set; }
            public string alias { get; set; }
            public DefinitionReference definitionReference { get; set; }
            public bool isPrimary { get; set; }
        }

        public class TriggerCondition
        {
            public string sourceBranch { get; set; }
            public IList<object> tags { get; set; }
            public bool useBuildDefinitionBranch { get; set; }
        }

        public class Trigger
        {
            public string artifactAlias { get; set; }
            public IList<TriggerCondition> triggerConditions { get; set; }
            public string triggerType { get; set; }
        }
        public class Web
        {
            public string href { get; set; }
        }
        public class Response
        {
            public string source { get; set; }
            public string name { get; set; }
            public object description { get; set; }
            public bool isDeleted { get; set; }
            public string path { get; set; }
            public Variables variables { get; set; }
            public IList<object> variableGroups { get; set; }
            public IList<Environment> environments { get; set; }
            public IList<Artifact> artifacts { get; set; }
            public IList<Trigger> triggers { get; set; }
            public string releaseNameFormat { get; set; }
            public IList<object> tags { get; set; }
            public Properties properties { get; set; }
        }
    }
}

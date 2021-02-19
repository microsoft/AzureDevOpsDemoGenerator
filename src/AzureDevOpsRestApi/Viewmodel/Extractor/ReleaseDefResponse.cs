using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{

    public class ReleaseDefResponse
    {
        public class HostingPlan
        {
            public string Value { get; set; }
        }

        public class ResourceGroupName
        {
            public string Value { get; set; }
        }

        public class ServerName
        {
            public string Value { get; set; }
        }

        public class Variables
        {
            public HostingPlan HostingPlan { get; set; }
            public ResourceGroupName ResourceGroupName { get; set; }
            public ServerName ServerName { get; set; }
        }

        public class Owner
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string UniqueName { get; set; }
        }

        public class PreApproval
        {
            public int Rank { get; set; }
            public bool IsAutomated { get; set; }
            public bool IsNotificationOn { get; set; }
        }

        public class PreDeployApprovals
        {
            public IList<PreApproval> Approvals { get; set; }
        }

        public class DeployStep
        {
        }

        public class PostApproval
        {
            public int Rank { get; set; }
            public bool IsAutomated { get; set; }
            public bool IsNotificationOn { get; set; }
        }

        public class PostDeployApprovals
        {
            public IList<PostApproval> Approvals { get; set; }
        }

        public class ParallelExecution
        {
            public string ParallelExecutionType { get; set; }
        }

        public class ArtifactsDownloadInput
        {
            public IList<object> DownloadInputs { get; set; }
        }

        public class OverrideInputs
        {
        }

        public class DeploymentInput
        {
            public ParallelExecution ParallelExecution { get; set; }
            public bool SkipArtifactsDownload { get; set; }
            public ArtifactsDownloadInput ArtifactsDownloadInput { get; set; }
            public string QueueId { get; set; }
            public IList<object> Demands { get; set; }
            public bool EnableAccessToken { get; set; }
            public int TimeoutInMinutes { get; set; }
            public int JobCancelTimeoutInMinutes { get; set; }
            public string Condition { get; set; }
            public OverrideInputs OverrideInputs { get; set; }
        }

        public class Inputs
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string ConnectedServiceName { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string ConnectedServiceNameArm { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Action { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string ResourceGroupName { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Location { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string TemplateLocation { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string CsmFileLink { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string CsmParametersFileLink { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string CsmFile { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string CsmParametersFile { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string OverrideParameters { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string DeploymentMode { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string EnableDeploymentPrerequisites { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string DeploymentGroupEndpoint { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string Project { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string DeploymentGroupName { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string CopyAzureVmTags { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string OutputVariable { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string WebAppName { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string WebAppKind { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string DeployToSlotFlag { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

            public string ImageSource { get; set; }
          
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
            public string JsonFiles { get; set; }
        }

        public class WorkflowTask
        {
            public string TaskId { get; set; }
            public string Version { get; set; }
            public string Name { get; set; }
            public string RefName { get; set; }
            public bool Enabled { get; set; }
            public bool AlwaysRun { get; set; }
            public bool ContinueOnError { get; set; }
            public int TimeoutInMinutes { get; set; }
            public string DefinitionType { get; set; }
            public OverrideInputs OverrideInputs { get; set; }
            public string Condition { get; set; }
            public Inputs Inputs { get; set; }
        }

        public class DeployPhas
        {
            public DeploymentInput DeploymentInput { get; set; }
            public int Rank { get; set; }
            public string PhaseType { get; set; }
            public string Name { get; set; }
            public IList<WorkflowTask> WorkflowTasks { get; set; }
        }

        public class EnvironmentOptions
        {
            public string EmailNotificationType { get; set; }
            public string EmailRecipients { get; set; }
            public bool SkipArtifactsDownload { get; set; }
            public int TimeoutInMinutes { get; set; }
            public bool EnableAccessToken { get; set; }
            public bool PublishDeploymentStatus { get; set; }
        }

        public class Condition
        {
            public string Name { get; set; }
            public string ConditionType { get; set; }
            public string Value { get; set; }
        }

        public class ExecutionPolicy
        {
            public int ConcurrencyCount { get; set; }
            public int QueueDepthCount { get; set; }
        }

        public class RetentionPolicy
        {
            public int DaysToKeep { get; set; }
            public int ReleasesToKeep { get; set; }
            public bool RetainBuild { get; set; }
        }

        public class ProcessParameters
        {
        }

        public class Properties
        {
        }

        public class PreDeploymentGates
        {
            public object GatesOptions { get; set; }
            public IList<object> Gates { get; set; }
        }

        public class PostDeploymentGates
        {
            public object GatesOptions { get; set; }
            public IList<object> Gates { get; set; }
        }

        public class Environment
        {
            public string Name { get; set; }
            public int Rank { get; set; }
            public Owner Owner { get; set; }
            public Variables Variables { get; set; }
            public IList<object> VariableGroups { get; set; }
            public PreDeployApprovals PreDeployApprovals { get; set; }
            public DeployStep DeployStep { get; set; }
            public PostDeployApprovals PostDeployApprovals { get; set; }
            public IList<DeployPhas> DeployPhases { get; set; }
            public EnvironmentOptions EnvironmentOptions { get; set; }
            public IList<object> Demands { get; set; }
            public IList<Condition> Conditions { get; set; }
            public ExecutionPolicy ExecutionPolicy { get; set; }
            public IList<object> Schedules { get; set; }
            public RetentionPolicy RetentionPolicy { get; set; }
            public ProcessParameters ProcessParameters { get; set; }
            public Properties Properties { get; set; }
            public PreDeploymentGates PreDeploymentGates { get; set; }
            public PostDeploymentGates PostDeploymentGates { get; set; }
        }

        public class DefaultVersionBranch
        {
            public string Name { get; set; }
        }

        public class DefaultVersionSpecific
        {
            public string Name { get; set; }
        }

        public class DefaultVersionTags
        {
            public string Name { get; set; }
        }

        public class DefaultVersionType
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class Definition
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
        public class Project
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
        public class DefinitionReference
        {
            public DefaultVersionBranch DefaultVersionBranch { get; set; }
            public DefaultVersionSpecific DefaultVersionSpecific { get; set; }
            public DefaultVersionTags DefaultVersionTags { get; set; }
            public DefaultVersionType DefaultVersionType { get; set; }
            public Definition Definition { get; set; }
            public Project Project { get; set; }
        }

        public class Artifact
        {
            public string SourceId { get; set; }
            public string Type { get; set; }
            public string Alias { get; set; }
            public DefinitionReference DefinitionReference { get; set; }
            public bool IsPrimary { get; set; }
        }

        public class TriggerCondition
        {
            public string SourceBranch { get; set; }
            public IList<object> Tags { get; set; }
            public bool UseBuildDefinitionBranch { get; set; }
        }

        public class Trigger
        {
            public string ArtifactAlias { get; set; }
            public IList<TriggerCondition> TriggerConditions { get; set; }
            public string TriggerType { get; set; }
        }
        public class Web
        {
            public string Href { get; set; }
        }
        public class Response
        {
            public string Source { get; set; }
            public string Name { get; set; }
            public object Description { get; set; }
            public bool IsDeleted { get; set; }
            public string Path { get; set; }
            public Variables Variables { get; set; }
            public IList<object> VariableGroups { get; set; }
            public IList<Environment> Environments { get; set; }
            public IList<Artifact> Artifacts { get; set; }
            public IList<Trigger> Triggers { get; set; }
            public string ReleaseNameFormat { get; set; }
            public IList<object> Tags { get; set; }
            public Properties Properties { get; set; }
        }
    }
}

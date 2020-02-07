using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.ReleaseDefinition
{
    public class ReleaseDefinitions
    {
        public class ReleaseDefinition
        {
            public string Name { get; set; }
            public object LastRelease { get; set; }
            public string Path { get; set; }
            public Variables Variables { get; set; }
            public IList<object> VariableGroups { get; set; }
            public IList<Environment> Environments { get; set; }
            public IList<Artifact> Artifacts { get; set; }
            public IList<Trigger> Triggers { get; set; }
            public string ReleaseNameFormat { get; set; }
        }
        public class Variables
        {
        }

        public class Owner
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string UniqueName { get; set; }
        }
        public class Approver
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string UniqueName { get; set; }
        }
        public class Approval
        {
            public int Rank { get; set; }
            public bool IsAutomated { get; set; }
            public bool IsNotificationOn { get; set; }
            public Approver Approver { get; set; }
        }
        public class PreDeployApprovals
        {
            public IList<Approval> Approvals { get; set; }
        }

        public class Environment
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Rank { get; set; }
            public Owner Owner { get; set; }
            public Variables Variables { get; set; }
            public PreDeployApprovals PreDeployApprovals { get; set; }
            public DeployStep DeployStep { get; set; }
            public PostDeployApprovals PostDeployApprovals { get; set; }
            public IList<DeployPhas> DeployPhases { get; set; }
            public EnvironmentOptions EnvironmentOptions { get; set; }
            public IList<object> Demands { get; set; }
            public string QueueId { get; set; }
            public IList<Condition> Conditions { get; set; }
            public ExecutionPolicy ExecutionPolicy { get; set; }
            public IList<object> Schedules { get; set; }
            public RetentionPolicy RetentionPolicy { get; set; }
        }
        public class DeployStep
        {
            public IList<Task> Tasks { get; set; }
        }

        public class DeploymentInput
        {
            public ParallelExecution ParallelExecution { get; set; }
            public bool SkipArtifactsDownload { get; set; }
            public int TimeoutInMinutes { get; set; }
            public string QueueId { get; set; }
            public IList<object> Demands { get; set; }
            public bool EnableAccessToken { get; set; }
            public bool Clean { get; set; }
            public string CleanOptions { get; set; }
        }
        public class DeployPhas
        {
            public DeploymentInput DeploymentInput { get; set; }
            public int Rank { get; set; }
            public string PhaseType { get; set; }
            public string Name { get; set; }
            public IList<object> WorkflowTasks { get; set; }
        }

        public class EnvironmentOptions
        {
            public string EmailNotificationType { get; set; }
            public string EmailRecipients { get; set; }
            public bool SkipArtifactsDownload { get; set; }
            public int TimeoutInMinutes { get; set; }
            public bool EnableAccessToken { get; set; }
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
        public class PostDeployApprovals
        {
            public IList<Approval> Approvals { get; set; }
        }

        public class ParallelExecution
        {
            public string ParallelExecutionType { get; set; }
        }


        public class Task
        {
            public string TaskId { get; set; }
            public string Version { get; set; }
            public string Name { get; set; }
            public bool Enabled { get; set; }
            public bool AlwaysRun { get; set; }
            public bool ContinueOnError { get; set; }
            public int TimeoutInMinutes { get; set; }
            public string DefinitionType { get; set; }
            public Inputs Inputs { get; set; }
        }
        public class Artifact
        {
            public string SourceId { get; set; }
            public string Type { get; set; }
            public string Alias { get; set; }
            public DefinitionReference DefinitionReference { get; set; }
            public bool IsPrimary { get; set; }
        }
        public class Inputs
        {
            public string ConnectedServiceName { get; set; }
            public string WebAppName { get; set; }
            public string DeployToSlotFlag { get; set; }
            public string ResourceGroupName { get; set; }
            public string SlotName { get; set; }
            public string Package { get; set; }
            public string SetParametersFile { get; set; }
            public string UseWebDeploy { get; set; }
            public string RemoveAdditionalFilesFlag { get; set; }
            public string ExcludeFilesFromAppDataFlag { get; set; }
            public string TakeAppOfflineFlag { get; set; }
            public string VirtualApplication { get; set; }
            public string AdditionalArguments { get; set; }
            public string WebAppUri { get; set; }
            public string ConnectedServiceNameSelector { get; set; }
            public string ConnectedServiceNameArm { get; set; }
            public string ServerName { get; set; }
            public string DatabaseName { get; set; }
            public string SqlUsername { get; set; }
            public string SqlPassword { get; set; }
            public string TaskNameSelector { get; set; }
            public string DacpacFile { get; set; }
            public string SqlFile { get; set; }
            public string SqlInline { get; set; }
            public string PublishProfile { get; set; }
            public string SqlAdditionalArguments { get; set; }
            public string InlineAdditionalArguments { get; set; }
            public string IpDetectionMethod { get; set; }
            public string StartIpAddress { get; set; }
            public string EndIpAddress { get; set; }
            public string DeleteFirewallRule { get; set; }
            public string AppId { get; set; }
            public string BinaryPath { get; set; }
            public string SymbolsPath { get; set; }
            public string NativeLibraryPath { get; set; }
            public string NotesPath { get; set; }
            public string Notes { get; set; }
            public string Publish { get; set; }
            public string Mandatory { get; set; }
            public string Notify { get; set; }
            public string Tags { get; set; }
            public string Teams { get; set; }
            public string Users { get; set; }
            public string AuthType { get; set; }
            public string ServiceEndpoint { get; set; }
            public string ServiceAccountKey { get; set; }
            public string ApkFile { get; set; }
            public string Track { get; set; }
            public string UserFraction { get; set; }
            public string ChangeLogFile { get; set; }
            public string ShouldAttachMetadata { get; set; }
            public string MetadataRootPath { get; set; }
            public string AdditionalApks { get; set; }
            public string MinDelta { get; set; }
            public string Operator { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Filename { get; set; }
            public string Arguments { get; set; }
            public string ModifyEnvironment { get; set; }
            public string WorkingFolder { get; set; }
            public string FailOnStandardError { get; set; }
            public string TestMachineGroup { get; set; }
            public string DropLocation { get; set; }
            public string TestSelection { get; set; }
            public string TestPlan { get; set; }
            public string TestSuite { get; set; }
            public string TestConfiguration { get; set; }
            public string Sourcefilters { get; set; }
            public string TestFilterCriteria { get; set; }
            public string RunSettingsFile { get; set; }
            public string OverrideRunParams { get; set; }
            public string CodeCoverageEnabled { get; set; }
            public string CustomSlicingEnabled { get; set; }
            public string TestRunTitle { get; set; }
            public string Platform { get; set; }
            public string Configuration { get; set; }
            public string TestConfigurations { get; set; }
            public string AutMachineGroup { get; set; }
            public string WebsiteUrl { get; set; }
            public string TestName { get; set; }
            public string VuLoad { get; set; }
            public string RunDuration { get; set; }
            public string GeoLocation { get; set; }
            public string MachineType { get; set; }
            public string AvgResponseTimeThreshold { get; set; }
            public string ConnectedServiceNameClassic { get; set; }
            public string Action { get; set; }
            public string ActionClassic { get; set; }
            public string CloudService { get; set; }
            public string Location { get; set; }
            public string CsmFile { get; set; }
            public string CsmParametersFile { get; set; }
            public string OverrideParameters { get; set; }
            [JsonProperty(PropertyName = "Deployment Mode")]
            public string DeploymentMode { get; set; }
            public string EnableDeploymentPrerequisitesForCreate { get; set; }
            public string EnableDeploymentPrerequisitesForSelect { get; set; }
            public string OutputVariable { get; set; }
            public string WebSiteLocation { get; set; }
            public string WebSiteName { get; set; }
            public string Slot { get; set; }
            public string DoNotDelete { get; set; }
            public string AppIdentifier { get; set; }
            public string IpaPath { get; set; }
            public string ReleaseTrack { get; set; }
            public string SkipBinaryUpload { get; set; }
            public string UploadMetadata { get; set; }
            public string MetadataPath { get; set; }
            public string UploadScreenshots { get; set; }
            public string ScreenshotsPath { get; set; }
            public string ShouldSubmitForReview { get; set; }
            public string ShouldAutoRelease { get; set; }
            public string ReleaseNotes { get; set; }
            public string ShouldSkipWaitingForProcessing { get; set; }
            public string ShouldSkipSubmission { get; set; }
            public string TeamId { get; set; }
            public string TeamName { get; set; }

        }
        public class DefinitionReference
        {
            public Definition Definition { get; set; }
            public Project Project { get; set; }
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
        public class Trigger
        {
            public string ArtifactAlias { get; set; }
            public string TriggerType { get; set; }
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.ViewModel
{
    public class ReleaseDefinitions
    {
        public class ReleaseDefinition
        {
            public string name { get; set; }
            public object lastRelease { get; set; }
            public string path { get; set; }
            public Variables variables { get; set; }
            public IList<object> variableGroups { get; set; }
            public IList<Environment> environments { get; set; }
            public IList<Artifact> artifacts { get; set; }
            public IList<Trigger> triggers { get; set; }
            public string releaseNameFormat { get; set; }
        }
        public class Variables
        {
        }
      
        public class Owner
        {
            public string id { get; set; }
            public string displayName { get; set; }
            public string uniqueName { get; set; }
        }
        public class Approver
        {
            public string id { get; set; }
            public string displayName { get; set; }
            public string uniqueName { get; set; }
        }
        public class Approval
        {
            public int rank { get; set; }
            public bool isAutomated { get; set; }
            public bool isNotificationOn { get; set; }
            public Approver approver { get; set; }
        }
        public class PreDeployApprovals
        {
            public IList<Approval> approvals { get; set; }
        }

        public class Environment
        {
            public string name { get; set; }
            public int rank { get; set; }
            public Owner owner { get; set; }
            public Variables variables { get; set; }
            public PreDeployApprovals preDeployApprovals { get; set; }
            public DeployStep deployStep { get; set; }
            public PostDeployApprovals postDeployApprovals { get; set; }
            public IList<DeployPhas> deployPhases { get; set; }
            public EnvironmentOptions environmentOptions { get; set; }
            public IList<object> demands { get; set; }
            public string queueId { get; set; }
            public IList<Condition> conditions { get; set; }
            public ExecutionPolicy executionPolicy { get; set; }
            public IList<object> schedules { get; set; }
            public RetentionPolicy retentionPolicy { get; set; }
        }
        public class DeployStep
        {
            public IList<Task> tasks { get; set; }
        }

        public class DeploymentInput
        {
            public ParallelExecution parallelExecution { get; set; }
            public bool skipArtifactsDownload { get; set; }
            public int timeoutInMinutes { get; set; }
            public string queueId { get; set; }
            public IList<object> demands { get; set; }
            public bool enableAccessToken { get; set; }
            public bool clean { get; set; }
            public string cleanOptions { get; set; }
        }
        public class DeployPhas
        {
            public DeploymentInput deploymentInput { get; set; }
            public int rank { get; set; }
            public string phaseType { get; set; }
            public string name { get; set; }
            public IList<object> workflowTasks { get; set; }
        }

        public class EnvironmentOptions
        {
            public string emailNotificationType { get; set; }
            public string emailRecipients { get; set; }
            public bool skipArtifactsDownload { get; set; }
            public int timeoutInMinutes { get; set; }
            public bool enableAccessToken { get; set; }
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
        public class PostDeployApprovals
        {
            public IList<Approval> approvals { get; set; }
        }

        public class ParallelExecution
        {
            public string parallelExecutionType { get; set; }
        }
       
       
        public class Task
        {
            public string taskId { get; set; }
            public string version { get; set; }
            public string name { get; set; }
            public bool enabled { get; set; }
            public bool alwaysRun { get; set; }
            public bool continueOnError { get; set; }
            public int timeoutInMinutes { get; set; }
            public string definitionType { get; set; }
            public Inputs inputs { get; set; }
        }
        public class Artifact
        {
            public string sourceId { get; set; }
            public string type { get; set; }
            public string alias { get; set; }
            public DefinitionReference definitionReference { get; set; }
            public bool isPrimary { get; set; }
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
            public string ConnectedServiceNameARM { get; set; }
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
            public string appID { get; set; }
            public string binaryPath { get; set; }
            public string symbolsPath { get; set; }
            public string nativeLibraryPath { get; set; }
            public string notesPath { get; set; }
            public string notes { get; set; }
            public string publish { get; set; }
            public string mandatory { get; set; }
            public string notify { get; set; }
            public string tags { get; set; }
            public string teams { get; set; }
            public string users { get; set; }
            public string authType { get; set; }
            public string serviceEndpoint { get; set; }
            public string serviceAccountKey { get; set; }
            public string apkFile { get; set; }
            public string track { get; set; }
            public string userFraction { get; set; }
            public string changeLogFile { get; set; }
            public string shouldAttachMetadata { get; set; }
            public string metadataRootPath { get; set; }
            public string additionalApks { get; set; }
            public string minDelta { get; set; }
            public string Operator { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string filename { get; set; }
            public string arguments { get; set; }
            public string modifyEnvironment { get; set; }
            public string workingFolder { get; set; }
            public string failOnStandardError { get; set; }
            public string testMachineGroup { get; set; }
            public string dropLocation { get; set; }
            public string testSelection { get; set; }
            public string testPlan { get; set; }
            public string testSuite { get; set; }
            public string testConfiguration { get; set; }
            public string sourcefilters { get; set; }
            public string testFilterCriteria { get; set; }
            public string runSettingsFile { get; set; }
            public string overrideRunParams { get; set; }
            public string codeCoverageEnabled { get; set; }
            public string customSlicingEnabled { get; set; }
            public string testRunTitle { get; set; }
            public string platform { get; set; }
            public string configuration { get; set; }
            public string testConfigurations { get; set; }
            public string autMachineGroup { get; set; }
            public string connectedServiceName { get; set; }
            public string websiteUrl { get; set; }
            public string testName { get; set; }
            public string vuLoad { get; set; }
            public string runDuration { get; set; }
            public string geoLocation { get; set; }
            public string machineType { get; set; }
            public string avgResponseTimeThreshold { get; set; }
            public string ConnectedServiceNameClassic { get; set; }
            public string action { get; set; }
            public string actionClassic { get; set; }
            public string resourceGroupName { get; set; }
            public string cloudService { get; set; }
            public string location { get; set; }
            public string csmFile { get; set; }
            public string csmParametersFile { get; set; }
            public string overrideParameters { get; set; }
            [JsonProperty(PropertyName = "Deployment Mode")]
            public string deploymentMode { get; set; }
            public string enableDeploymentPrerequisitesForCreate { get; set; }
            public string enableDeploymentPrerequisitesForSelect { get; set; }
            public string outputVariable { get; set; }
            public string WebSiteLocation { get; set; }
            public string WebSiteName { get; set; }
            public string Slot { get; set; }
            public string doNotDelete { get; set; }
            public string appIdentifier { get; set; }
            public string ipaPath { get; set; }
            public string releaseTrack { get; set; }
            public string skipBinaryUpload { get; set; }
            public string uploadMetadata { get; set; }
            public string metadataPath { get; set; }
            public string uploadScreenshots { get; set; }
            public string screenshotsPath { get; set; }
            public string shouldSubmitForReview { get; set; }
            public string shouldAutoRelease { get; set; }
            public string releaseNotes { get; set; }
            public string shouldSkipWaitingForProcessing { get; set; }
            public string shouldSkipSubmission { get; set; }
            public string teamId { get; set; }
            public string teamName { get; set; }
          
        }
        public class DefinitionReference
        {
            public Definition definition { get; set; }
            public Project project { get; set; }
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
        public class Trigger
        {
            public string artifactAlias { get; set; }
            public string triggerType { get; set; }
        }
    }
}


 
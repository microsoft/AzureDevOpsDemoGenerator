using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TemplatesGeneratorTool.ViewModel
{
	public class PlanViewModel
	{
		public class Value
		{
			public string id { get; set; }
			public string name { get; set; }
		}

		public class AllPlans
		{
			public int count { get; set; }
			public IList<Value> value { get; set; }
		}

		public class TeamBacklogMapping
		{
			public string teamId { get; set; }
			public string categoryReferenceName { get; set; }
		}
		public class Criterion
		{
			public string fieldName { get; set; }
			public string logicalOperator { get; set; }

			[JsonProperty(PropertyName = "operator")]
			public string Operator { get; set; }
			public string value { get; set; }
		}
		public class CoreField
		{
			public string referenceName { get; set; }
			public string displayName { get; set; }
			public string fieldType { get; set; }
			public bool isIdentity { get; set; }
		}
		public class Fields
		{
			public bool showAssignedTo { get; set; }
			public string assignedToDisplayFormat { get; set; }
			public bool showTags { get; set; }
			public bool showState { get; set; }
			public IList<CoreField> coreFields { get; set; }
		}
		public class CardSettings
		{
			public Fields fields { get; set; }
		}
		public class Properties
		{
			public IList<TeamBacklogMapping> teamBacklogMappings { get; set; }
			public IList<Criterion> criteria { get; set; }
			public CardSettings cardSettings { get; set; }
		}
		public class DeliveryPlan
		{

			public string name { get; set; }
			public string type { get; set; }
			public Properties properties { get; set; }

		}
	}
}

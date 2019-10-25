using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.ViewModel
{
    class WidgetAndChartResponse
    {
        public class Position
        {
            public int row { get; set; }
            public int column { get; set; }
        }

        public class Size
        {
            public int rowSpan { get; set; }
            public int columnSpan { get; set; }
        }
        public class SettingsVersion
        {
            public int major { get; set; }
            public int minor { get; set; }
            public int patch { get; set; }
        }
        public class Dashboard
        {
            public string eTag = "$eTag$";
        }
        public class Value
        {
           
            public string name { get; set; }
            public Position position { get; set; }
            public Size size { get; set; }
            public string settings { get; set; }
            public SettingsVersion settingsVersion { get; set; }
            public string contributionId { get; set; }
            public Dashboard dashboard = new Dashboard() { eTag = "$eTag$" };
        }
        public class Widget
        {
            public int count { get; set; }
            public IList<Value> value { get; set; }
        }
    }
}

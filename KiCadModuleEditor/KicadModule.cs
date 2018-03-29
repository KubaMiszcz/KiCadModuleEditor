using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiCadModuleEditor
{
	class KicadModule
	{
		public string Name;
		public string FileName;
		public string Path;
		public string Value;
		public string Content;
		List<string> ContentList;
		public string LinkToDatasheet;
		public List<string> KeywordsList;
		public string LinkTo3DModel;
	}
}

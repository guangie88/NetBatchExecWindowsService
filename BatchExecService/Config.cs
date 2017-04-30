using System.Collections.Generic;
using System.Diagnostics;

namespace BatchExecService
{
    public class Config
    {
        public List<CmdGroup> CmdGroups { get; set; }
    }

    public class CmdGroup
    {
        public string FileName { get; set; }
        public string Arguments { get; set; }
        public ProcessWindowStyle WindowStyle { get; set; }
    }
}

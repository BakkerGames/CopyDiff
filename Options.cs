using System.Collections.Generic;

namespace CopyDiff
{
    public class Options
    {
        public bool LongFilenames { get; set; } = false;
        public List<string> ExcludeDirs = new List<string>();
        public int FoundCount { get; set; } = 0;
        public int CopyCount { get; set; } = 0;
    }
}

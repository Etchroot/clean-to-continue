using System;

namespace CleanToContinue.Core
{
    public sealed class ToolSelectionModel
    {
        public CleaningTool Selected { get; private set; } = CleaningTool.AirGun;

        public event Action<CleaningTool> SelectionChanged;

        public void Select(CleaningTool tool)
        {
            if (Selected == tool)
            {
                return;
            }

            Selected = tool;
            SelectionChanged?.Invoke(tool);
        }
    }
}

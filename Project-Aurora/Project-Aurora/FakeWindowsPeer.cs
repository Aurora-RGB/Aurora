using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;

namespace AuroraRgb;

public class FakeWindowsPeer(Window window) : WindowAutomationPeer(window)
{
    protected override List<AutomationPeer> GetChildrenCore()
    {
        return [];
    }
}
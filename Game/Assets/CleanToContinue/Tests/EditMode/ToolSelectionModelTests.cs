using CleanToContinue.Core;
using NUnit.Framework;

namespace CleanToContinue.Tests.EditMode
{
    public sealed class ToolSelectionModelTests
    {
        [Test]
        public void StartsWithAirGun()
        {
            Assert.That(
                new ToolSelectionModel().Selected,
                Is.EqualTo(CleaningTool.AirGun));
        }

        [Test]
        public void SelectingSameToolTwiceRaisesOneChange()
        {
            var model = new ToolSelectionModel();
            var calls = 0;
            model.SelectionChanged += _ => calls++;

            model.Select(CleaningTool.Cloth);
            model.Select(CleaningTool.Cloth);

            Assert.That(model.Selected, Is.EqualTo(CleaningTool.Cloth));
            Assert.That(calls, Is.EqualTo(1));
        }
    }
}

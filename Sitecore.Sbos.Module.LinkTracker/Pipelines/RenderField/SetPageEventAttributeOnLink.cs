using Sitecore.Pipelines.RenderField;
using Sitecore.Sbos.Module.LinkTracker.Data.Constants;

namespace Sitecore.Sbos.Module.LinkTracker.Pipelines.RenderField
{
    public class SetPageEventAttributeOnLink : SetTrackedAttributeOnLink
    {
        public override void Process(RenderFieldArgs args)
        {
            if (!this.CanProcess(args))
                return;

            bool shouldTrigger = "1" == (this.GetXmlAttributeValue(args.FieldValue, LinkTrackerConstants.PageEventTriggerAttName) ?? "");
            if (shouldTrigger)
                args.Result.FirstPart = this.AddOrExtendAttributeValue(args.Result.FirstPart, "onclick", $"triggerPageEvent('{this.GetXmlAttributeValue(args.FieldValue, this.XmlAttributeName)}', 'true', '{this.GetXmlAttributeValue(args.FieldValue, LinkTrackerConstants.PageEventDataAttName)}');");
        }
    }
}
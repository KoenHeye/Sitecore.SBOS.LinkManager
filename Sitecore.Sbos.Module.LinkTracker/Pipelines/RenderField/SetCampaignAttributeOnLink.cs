using Sitecore.Pipelines.RenderField;
using Sitecore.Sbos.Module.LinkTracker.Data.Constants;

namespace Sitecore.Sbos.Module.LinkTracker.Pipelines.RenderField
{
    public class SetCampaignAttributeOnLink : SetTrackedAttributeOnLink
    {
        public override void Process(RenderFieldArgs args)
        {
            if (!this.CanProcess(args))
                return;

            bool shouldTrigger = "1" == (this.GetXmlAttributeValue(args.FieldValue, LinkTrackerConstants.CampaignTriggerAttName) ?? "");
            if (shouldTrigger)
                args.Result.FirstPart = this.AddOrExtendAttributeValue(args.Result.FirstPart, "onclick", $"triggerCampaign('{this.GetXmlAttributeValue(args.FieldValue, this.XmlAttributeName)}', 'true', '{this.GetXmlAttributeValue(args.FieldValue, LinkTrackerConstants.CampaignDataAttName)}');"); 
        }
    }
}
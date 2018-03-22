using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Sitecore.Analytics;
using Sitecore.Analytics.Data.Items;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Analytics.Data;

namespace Sitecore.Sbos.Module.LinkTracker.Events.Handler
{
    public class TrackedLinkHandler : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable => false;
        public bool campaigncheck = false;

        [HttpGet]
        public void ProcessRequest(HttpContext context)
        {
            this.HandleQueryStringParameter(context, "triggerCampaign", "cid", "campaignData");
            this.HandleQueryStringParameter(context, "triggerGoal", "gid", "goalData");
            this.HandleQueryStringParameter(context, "triggerPageEvent", "peid", "pageEventData");
        }

        private void HandleQueryStringParameter(HttpContext context, string triggerParam, string idParam, string dataParam)
        {
            var parameter = context.Request.QueryString[triggerParam];
            if (parameter == null) return;

            bool shouldTrigger;
            bool.TryParse(parameter, out shouldTrigger);
            if (!shouldTrigger) return;

            if (triggerParam != "triggerCampaign")
            {
                var id = context.Request.QueryString[idParam];
                var data = context.Request.QueryString[dataParam];
                this.TriggerEvent(id, data);
            }

            if (triggerParam == "triggerCampaign")
            {
                var cid = context.Request.QueryString[idParam];
                var cdata = context.Request.QueryString[dataParam];
                this.TriggerCampaign(cid, cdata);
            }
        }

        private void TriggerEvent(string id, string data)
        {
            ID scId;

            if (!string.IsNullOrEmpty(id) && ID.TryParse(id, out scId))
            {
                if (!Tracker.IsActive)
                    Tracker.StartTracking();               
                
                if (Tracker.Current.CurrentPage != null && Tracker.Current.Interaction != null)
                {
                    //PageEvent
                    Item defItem = Context.Database.GetItem(scId);
                    var eventToTrigger = new PageEventData(defItem.Name, scId.Guid)
                    {
                        Data = data
                    };
                    
                    if (Tracker.Current.Interaction.PreviousPage != null)
                        //Previous page = the actual page where the event was triggered
                        Tracker.Current.Interaction.PreviousPage.Register(eventToTrigger);
                    else
                        //Current page will be this TrackedLinkHandler
                        Tracker.Current.CurrentPage.Register(eventToTrigger);
                }
            }
        }

         private void TriggerCampaign(string cid, string cdata)
         {
             ID scId;

             if (!string.IsNullOrEmpty(cid) && ID.TryParse(cid, out scId))
             {
                 if (!Tracker.IsActive)
                    Tracker.StartTracking();

                 if (Tracker.Current.CurrentPage != null && Tracker.Current.Interaction != null)
                 {                     
                     Item campaignItem = Context.Database.GetItem(cid);
                     CampaignItem campaignToTrigger = new CampaignItem(campaignItem)
                     {
                         Data = cdata
                     };

                     if (Tracker.Current.Interaction.PreviousPage != null)
                         //Previous page = the actual page where the event was triggered
                         Tracker.Current.Interaction.PreviousPage.TriggerCampaign(campaignToTrigger);
                     else
                         //Current page will be this TrackedLinkHandler
                         Tracker.Current.CurrentPage.TriggerCampaign(campaignToTrigger);
                }
             }
         }       
    }
}

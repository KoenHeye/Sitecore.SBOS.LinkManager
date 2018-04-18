using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Sitecore.Analytics;
using Sitecore.Analytics.Data.Items;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Analytics.Data;
using Sitecore.Analytics.Outcome.Extensions;
using Sitecore.Analytics.Outcome.Model;
using Sitecore.Common;
using Sitecore.Diagnostics;

namespace Sitecore.Sbos.Module.LinkTracker.Events.Handler
{
    public class TrackedLinkHandler : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable => false;

        private readonly Dictionary<string, Action<ID, string>> _actionMap;
        
        public TrackedLinkHandler()
        {
            _actionMap = new Dictionary<string, Action<ID, string>>
            {
                { "goal", TriggerEvent },
                { "event", TriggerEvent },
                { "campaign", TriggerCampaign },
                { "outcome", TriggerOutcome }
            };
        }
        

        [HttpGet]
        public void ProcessRequest(HttpContext context)
        {
            if (ValidateAndTrigger(context))
                return;

            //fallback to older methods
            this.HandleQueryStringParameter(context, "triggerCampaign", "cid", "campaignData");
            this.HandleQueryStringParameter(context, "triggerGoal", "gid", "goalData");
            this.HandleQueryStringParameter(context, "triggerPageEvent", "peid", "pageEventData");
        }


        [Obsolete("Let ValidateAndTrigger with only context as parameter handle the received data.")]
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

        [Obsolete("Use TriggerEvent with ID as parameter.")]
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

        [Obsolete("Use TriggerCampaign with ID as parameter.")]
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

        /// <summary>
        /// Handles the request and will parse type, id and data from the querystring.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>True if the type is known and an id was received.</returns>
        private bool ValidateAndTrigger(HttpContext context)
        {
            //Validate type
            var type = context.Request.QueryString["type"];
            if (string.IsNullOrEmpty(type)) return false;
            //Validate actionMap for type. If not, it is possible a legacy call.
            if (!_actionMap.ContainsKey(type)) return false;

            //Validate the Id
            var id = context.Request.QueryString["id"];
            if (string.IsNullOrEmpty(id)) return false;
            if (!ID.TryParse(id, out ID scId)) return false;

            var data = context.Request.QueryString["data"];

            //Execute corresponding method
            _actionMap[type](scId, data ?? "");
            return true;
        }

        /// <summary>
        /// Used for tracking events and goals
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        private void TriggerEvent(ID id, string data = "")
        {
            try
            {
                //start tracker if not active
                if (!Tracker.IsActive)
                    Tracker.StartTracking();

                //additional check to ensure that Tracker is active and has a CurrentPage 
                if (!Tracker.IsActive || Tracker.Current?.CurrentPage == null)
                    return;

                // Look up the goal item
                var goalItem = Sitecore.Context.Database.GetItem(id);
                if (goalItem == null)
                    Log.Error($"Goal could not be found: {id}.", typeof(TrackedLinkHandler));
                else
                {
                    var goal = new PageEventItem(goalItem); // Wrap goal in a PageEventItem
                    Analytics.Model.PageEventData pageEventsRow;  // Create PageEventData for the goal to be stored

                    if (Tracker.Current.Interaction.PreviousPage != null)
                        //Previous page = the actual page where the event was triggered
                        pageEventsRow = Tracker.Current.Interaction.PreviousPage.Register(goal);
                    else
                        //Current page will be this TrackedLinkHandler
                        pageEventsRow = Tracker.Current.CurrentPage.Register(goal);
                                        
                    pageEventsRow.Data = data;
                    Tracker.Current.Interaction.AcceptModifications(); //updates current interaction's end date time
                }
            }
            catch (Exception ex)
            {
                //if you can't track log a warning but do impact user for analytics issues
                Log.Warn($"Failed to trigger goal: {ex}", typeof(TrackedLinkHandler));
            }
        }

        private void TriggerCampaign(ID id, string data = "")
        {
            try
            {
                //start tracker if not active
                if (!Tracker.IsActive)
                    Tracker.StartTracking();

                //additional check to ensure that Tracker is active and has a CurrentPage 
                if (!Tracker.IsActive || Tracker.Current?.CurrentPage == null)
                    return;

                Item campaignItem = Context.Database.GetItem(id);
                if(campaignItem == null)
                    Log.Error($"Goal could not be found: {id}.", typeof(TrackedLinkHandler));
                else
                {
                    CampaignItem campaignToTrigger = new CampaignItem(campaignItem)
                    {
                        Data = data
                    };

                    if (Tracker.Current.Interaction.PreviousPage != null)
                        //Previous page = the actual page where the event was triggered
                        Tracker.Current.Interaction.PreviousPage.TriggerCampaign(campaignToTrigger);
                    else
                        //Current page will be this TrackedLinkHandler
                        Tracker.Current.CurrentPage.TriggerCampaign(campaignToTrigger);
                }
            }
            catch (Exception ex)
            {
                //if you can't track log a warning but do impact user for analytics issues
                Log.Warn($"Failed to trigger goal: {ex}", typeof(TrackedLinkHandler));
            }
        }

        private void TriggerOutcome(ID id, string data = "")
        {
            Assert.IsNotNull(id, "outcomeDefinitionId != null");

            try
            {
                //start tracker if not active
                if (!Tracker.IsActive)
                    Tracker.StartTracking();

                //additional check to ensure that Tracker is active and has a Contact
                if (!Tracker.IsActive || Tracker.Current?.Contact == null)
                    return;

                //outcomeId - The unique ID of the outcome in Sitecore.
                //definitionId - The Item ID of the outcome definition item in the Marketing Control Panel.
                //contactId - The unique ID of the contact stored in the collection database.
                var outcome = new ContactOutcome(new ID(), id, Tracker.Current.Contact.ContactId.ToID());

                Tracker.Current.RegisterContactOutcome(outcome);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to trigger outcome for id: {id}", e, typeof(TrackedLinkHandler));
            }
        }
    }
}

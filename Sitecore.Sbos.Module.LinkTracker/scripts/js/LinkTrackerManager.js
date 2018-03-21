var triggerCount = 0; GoalCheck = "", PageEventCheck = "", CampaignCheck = "", popupCheck = 0;

function triggerCampaign(campaignId, shouldTriggerCampaign, campaignData) {
    $.ajax({
        url: "/Events/Handler/TrackedLinkHandler.ashx",
        type: "GET",
        data: { cid: campaignId, triggerCampaign: shouldTriggerCampaign, campaignData },
        context: this,
        success: function (data) {

        },
        error: function (data) {
            console.error("SBOS LinkTracker: Campaign has not been triggered");
        }
    });
}

function triggerGoal(goalId, shouldTriggerGoal, goalData) {
    $.ajax({
        url: "/Events/Handler/TrackedLinkHandler.ashx",
        type: "GET",
        data: { gid: goalId, triggerGoal: shouldTriggerGoal, goalData },
        context: this,
        success: function (data) {

        },
        error: function (data) {
            console.error("SBOS LinkTracker: GoalEvent has not been triggered");
        }
    });
}

function triggerPageEvent(pageEventId, shouldTriggerPageEvent, pageEventData) {
    $.ajax({
        url: "/Events/Handler/TrackedLinkHandler.ashx",
        type: "GET",
        data: { peid: pageEventId, triggerPageEvent: shouldTriggerPageEvent, pageEventData },
        context: this,
        success: function (data) {           


        },
        error: function (data) {
            console.error("SBOS LinkTracker: PageEvent has not been triggered");
        }
    });
}


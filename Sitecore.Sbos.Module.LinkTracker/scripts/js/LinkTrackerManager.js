/**
 * Ajax call to backend handler to trigger and track Sitecore analytics data.
 * @param {string} type Should be goal, event, campaign or outcome.
 * @param {string} id The Sitecore id of the type to be triggered. Including its brackets. Eg. {9b363310-fddd-4e47-9f75-d976a20a07d7}
 * @param {string} data Any additional data that you want to store.
 */
function triggerLinkEvent(type, id, data) {
    $.ajax({
        url: "/Events/Handler/TrackedLinkHandler.ashx",
        type: "GET",
        data: { type, id, data},
        context: this
    }).fail(function (data)
    {
        console.error("SBOS LinkTracker error: " + data);
    });
}

/**
 * @deprecated Will be deleted in later version. Use triggerLinkEvent(type, id, data)
 */
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

/**
 * @deprecated Will be deleted in later version. Use triggerLinkEvent(type, id, data)
 */
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

/**
 * @deprecated Will be deleted in later version. Use triggerLinkEvent(type, id, data)
 */
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
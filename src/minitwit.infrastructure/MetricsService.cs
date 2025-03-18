using Prometheus;

namespace minitwit.web;

public class MetricsService
{
    private static readonly Counter RegisterCounter = Metrics
        .CreateCounter("minitwit_register_counter_total", "Total number of register requests received.");
    private static readonly Counter PostMsgsCounter = Metrics
        .CreateCounter("minitwit_postmsgs_counter_total", "Total number of msgs posted.");
    private static readonly Counter FollowCounter = Metrics
        .CreateCounter("minitwit_follow_counter_total", "Total number of follow requests received.");
    private static readonly Counter UnfollowCounter = Metrics
        .CreateCounter("minitwit_unfollow_counter_total", "Total number of unfollow requests received.");
    private static readonly Counter GetRequestsCounter = Metrics
        .CreateCounter("minitwit_getRequests_counter_total", "Total number of get requests received.");
    private static readonly Counter ErrorCounter = Metrics
            .CreateCounter("minitwit_ErrorCounter_counter_total", "Total number of Errors received.");
        
    private static readonly Counter UnfollowNeedToFollowCounter = Metrics
        .CreateCounter("minitwit_UnfollowNeedToFollow_counter_total", "Total number of UnfollowNeedToFollow error received.");
    private static readonly Counter UnfollowNoWhoOrWhomCounter = Metrics
        .CreateCounter("minitwit_UnfollowNoWhoOrWhom_counter_total", "Total number of unfollowNoWhoOrWhom error received.");
    private static readonly Counter UnfollowfollowerEntryNullCounter = Metrics
        .CreateCounter("minitwit_UnfollowfollowerEntryNull_counter_total", "Total number of UnfollowfollowerEntryNull error received.");

    private static readonly Histogram RequestDuration = Metrics
        .CreateHistogram("app_request_duration_seconds", "Histogram of request duration.");
    
    
    public void IncrementRegisterCounter()
    {
        RegisterCounter.Inc();
    }
    public void IncrementPostMsgsCounter()
    {
        PostMsgsCounter.Inc();
    }
    public void IncrementFollowCounter()
    {
        FollowCounter.Inc();
    }
    public void IncrementUnFollowCounter()
    {
        UnfollowCounter.Inc();
    }
    public void IncrementGetRequestsCounter()
    {
        GetRequestsCounter.Inc();
    }
    public void IncrementErrorCounter()
    {
        ErrorCounter.Inc();
    }
    public void IncrementUnfollowNeedToFollowCounter()
    {
        UnfollowNeedToFollowCounter.Inc();
    }
    public void IncrementUnfollowNoWhoOrWhomCounter()
    {
        UnfollowNoWhoOrWhomCounter.Inc();
    }
    public void IncrementUnfollowfollowerEntryNullCounter()
    {
        UnfollowfollowerEntryNullCounter.Inc();
    }
    

    // Method to observe request duration
    public IDisposable MeasureRequestDuration()
    {
        return RequestDuration.NewTimer();
    }
}
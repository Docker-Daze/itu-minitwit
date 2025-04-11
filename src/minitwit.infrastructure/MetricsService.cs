using Prometheus;

namespace minitwit.web;

public class MetricsService
{
    private static readonly Counter RegisterCounter = Metrics
        .CreateCounter("minitwit_register_counter", "Total number of register requests received.");
    private static readonly Counter PostMsgsCounter = Metrics
        .CreateCounter("minitwit_postmsgs_counter", "Total number of msgs posted.");
    private static readonly Counter FollowCounter = Metrics
        .CreateCounter("minitwit_follow_counter", "Total number of follow requests received.");
    private static readonly Counter UnfollowCounter = Metrics
        .CreateCounter("minitwit_unfollow_counter", "Total number of unfollow requests received.");
    private static readonly Counter GetRequestsCounter = Metrics
        .CreateCounter("minitwit_getRequests_counter", "Total number of get requests received.");
    
    
    private static readonly Histogram RequestDurationAVG = Metrics
        .CreateHistogram("app_request_duration_seconds", "Histogram of request duration.");
    private static readonly Histogram RequestDurationFollow = Metrics
        .CreateHistogram("app_RequestDurationFollow_seconds", "Histogram of RequestDurationFollow duration.");
    private static readonly Histogram RequestDurationUnfollow = Metrics
        .CreateHistogram("app_RequestDurationUnfollow_seconds", "Histogram of RequestDurationUnfollow duration.");
    private static readonly Histogram RequestDurationPostMsgs = Metrics
        .CreateHistogram("app_RequestDurationPostMsgs_seconds", "Histogram of RequestDurationPostMsgs duration.");
    private static readonly Histogram RequestDurationRegister = Metrics
        .CreateHistogram("app_RequestDurationRegister_seconds", "Histogram of RequestDurationRegister duration.");
    
    
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

    // Method to observe request duration
    public IDisposable MeasureRequestDuration()
    {
        return RequestDurationAVG.NewTimer();
    }
    public IDisposable MeasureRequestFollowDuration()
    {
        return RequestDurationFollow.NewTimer();
    }
    public IDisposable MeasureRequestUnfollowDuration()
    {
        return RequestDurationUnfollow.NewTimer();
    }
    public IDisposable MeasureRequestPostMsgsDuration()
    {
        return RequestDurationPostMsgs.NewTimer();
    }
    public IDisposable MeasureRequestRegisterDuration()
    {
        return RequestDurationRegister.NewTimer();
    }
    
}
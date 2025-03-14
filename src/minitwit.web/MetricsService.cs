using Prometheus;

namespace minitwit.web;

public class MetricsService
{
    private static readonly Counter RegisterCounter = Metrics
        .CreateCounter("minitwit_register_counter", "Total number of requests received.");
    private static readonly Counter PostMsgsCounter = Metrics
        .CreateCounter("minitwit_postmsgs_counter", "Total number of requests received.");
    private static readonly Counter FollowCounter = Metrics
        .CreateCounter("minitwit_follow_counter", "Total number of requests received.");
    private static readonly Counter UnfollowCounter = Metrics
        .CreateCounter("minitwit_unfollow_counter", "Total number of requests received.");
    

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
    

    // Method to observe request duration
    public IDisposable MeasureRequestDuration()
    {
        return RequestDuration.NewTimer();
    }
}
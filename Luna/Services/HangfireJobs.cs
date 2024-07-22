using Hangfire;

namespace Luna.Services
{
    public class HangfireJobs
    {
        public static void ScheduleJobs()
        {
            TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            RecurringJob.AddOrUpdate<JobService>(job => job.CheckDiscountsAndSendEmails(), "34 18 * * *", localTimeZone);
        }
    }

}

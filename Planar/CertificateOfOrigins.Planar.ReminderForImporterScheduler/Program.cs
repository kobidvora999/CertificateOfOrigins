using CertificateOfOrigins.Planar.ReminderForImporterScheduler.Jobs;
using Planar.Job;

// PlanarJob requires TJob to have a public parameterless constructor, which is why the job resolves its
// dependencies from ServiceProvider rather than through injection.
await PlanarJob.StartAsync<ReminderForImporterSchedulerJob>();

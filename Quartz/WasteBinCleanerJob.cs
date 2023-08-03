using Quartz;

namespace FilesAPI.Quartz
{
    public class WasteBinCleanerJob : IJob
    {
        private readonly AppContext _appContext;
        private readonly ILogger<WasteBinCleanerJob> _logger;

        public WasteBinCleanerJob(AppContext appContext, ILogger<WasteBinCleanerJob> logger)
        {
            _appContext = appContext;
            _logger = logger;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var fileList = _appContext.Files.Where(x => x.IsDeleted && DateTime.UtcNow - x.LastTimeChanged > TimeSpan.FromMinutes(30)).ToList();
            fileList.ForEach(x => _appContext.Remove(x));
            await _appContext.SaveChangesAsync();
            _logger.LogInformation("Из корзины удалены следующие файлы: {fileList}", fileList);
        }
    }
}

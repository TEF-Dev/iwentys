﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iwentys.Core;
using Iwentys.Core.GoogleTableIntegration.Marks;
using Iwentys.Database.Context;
using Iwentys.Models.Entities.Study;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Iwentys.Api.BackgroundServices
{
    public class MarkUpdateBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger _logger;

        public MarkUpdateBackgroundService(ILoggerFactory loggerFactory, IServiceProvider sp)
        {
            _sp = sp;
            _logger = loggerFactory.CreateLogger("MarkUpdateBackgroundService");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MarkUpdateBackgroundService start");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = _sp.CreateScope();
                    _logger.LogInformation("Execute MarkUpdateBackgroundService update");

                    var accessor = scope.ServiceProvider.GetRequiredService<DatabaseAccessor>();
                    var googleTableUpdateService = new MarkGoogleTableUpdateService(_logger, accessor.SubjectActivity, accessor.Student);

                    //TODO: wrap with try/cath for each update
                    List<GroupSubjectEntity> groups = accessor.GroupSubject.Read().ToList();
                    groups.ForEach(g => googleTableUpdateService.UpdateSubjectActivityForGroup(g));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Fail to perform MarkUpdateBackgroundService update");
                }

                await Task.Delay(ApplicationOptions.DaemonUpdateInterval, stoppingToken).ConfigureAwait(false);
            }
        }
    }
}
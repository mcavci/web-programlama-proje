using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SporSalonuProjesi.Data;

public class RandevuTemizlemeServisi : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public RandevuTemizlemeServisi(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var simdi = DateTime.Now;

                
              
                var eskiRandevular = context.Randevular
                    .Where(x => x.Tarih < simdi.Date &&
                                x.Durum != "Tamamlandı" &&
                                x.Durum != "İptal")
                    .ToList();

                if (eskiRandevular.Any())
                {
                
                    foreach (var randevu in eskiRandevular)
                    {
                        randevu.Durum = "Tamamlandı";
                    }

                    context.UpdateRange(eskiRandevular);
                    await context.SaveChangesAsync();
                
                }
            }

            
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}